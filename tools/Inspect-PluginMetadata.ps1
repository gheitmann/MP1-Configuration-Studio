param([string]$Runtime,[string]$Output)
$ErrorActionPreference='Stop'
$script:assemblies=@{}
$script:lookup=@{}
Get-ChildItem -LiteralPath $Runtime -Filter '*.dll' -Recurse -File | ForEach-Object {if(-not $script:lookup.ContainsKey($_.Name)){$script:lookup[$_.Name]=$_.FullName}}
$resolver=[ResolveEventHandler]{param($sender,$eventArgs) $name=([Reflection.AssemblyName]::new($eventArgs.Name)).Name+'.dll';if($script:lookup.ContainsKey($name)){return [Reflection.Assembly]::ReflectionOnlyLoadFrom($script:lookup[$name])};return [Reflection.Assembly]::ReflectionOnlyLoad($eventArgs.Name)}
[AppDomain]::CurrentDomain.add_ReflectionOnlyAssemblyResolve($resolver)
$doc=New-Object Xml.XmlDocument;$root=$doc.CreateElement('pluginMetadata');$root.SetAttribute('inspection','metadata only; no plugin constructors, setup methods or save handlers executed');$doc.AppendChild($root) | Out-Null
foreach($group in @('windows','process')){
 foreach($file in Get-ChildItem -LiteralPath (Join-Path $Runtime ('plugins/'+$group)) -Filter '*.dll' -File){
  $entry=$doc.CreateElement('assembly');$entry.SetAttribute('file',$file.Name);$entry.SetAttribute('group',$group);$root.AppendChild($entry) | Out-Null
  try{
   $assembly=[Reflection.Assembly]::ReflectionOnlyLoadFrom($file.FullName);$entry.SetAttribute('version',$assembly.GetName().Version.ToString())
   try{$types=$assembly.GetTypes()}catch [Reflection.ReflectionTypeLoadException]{$types=$_.Exception.Types | Where-Object {$_};$entry.SetAttribute('partial','true')}
   foreach($type in $types){
    if($type.IsAbstract){continue}
    $isForm=$false;try{$base=$type.BaseType;while($base){if($base.FullName -eq 'System.Windows.Forms.Form'){$isForm=$true;break};$base=$base.BaseType}}catch{}
    $setup=$false;try{$setup=@($type.GetInterfaces() | Where-Object {$_.FullName -eq 'MediaPortal.GUI.Library.ISetupForm'}).Count -gt 0}catch{}
    if($isForm -or $setup){$node=$doc.CreateElement($(if($isForm){'form'}else{'setup'}));$node.SetAttribute('type',$type.FullName);$node.SetAttribute('public',$type.IsPublic.ToString());if($isForm){$node.SetAttribute('parameterlessConstructor',(@($type.GetConstructors() | Where-Object {$_.GetParameters().Count -eq 0}).Count -gt 0).ToString())};$entry.AppendChild($node) | Out-Null}
   }
  }catch{$entry.SetAttribute('error',$_.Exception.GetType().Name)}
 }
}
$doc.Save($Output)
[pscustomobject]@{Assemblies=$doc.SelectNodes('//assembly').Count;Forms=$doc.SelectNodes('//form').Count;SetupProviders=$doc.SelectNodes('//setup').Count;Errors=$doc.SelectNodes('//assembly[@error]').Count;Partial=$doc.SelectNodes('//assembly[@partial]').Count}
