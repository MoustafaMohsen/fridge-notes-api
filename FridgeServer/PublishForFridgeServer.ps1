#Name Mohesen 2018

heroku container:login

$appname = "linux-docker-4"
$imagename = "fridge-server"

dotnet publish -c Release

Copy-Item ".\Dockerfile" -Destination ".\bin\release\netcoreapp2.0\publish"

& docker build -t $imagename ./bin/release/netcoreapp2.0/publish

& docker tag $imagename registry.heroku.com/$appname/web

#Confirmation dialogue
$caption1 = "Push Image"    
$message1 = ''

[int]$defaultChoice1 = 0
$yes1 = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes", "Push"
$no1 = New-Object System.Management.Automation.Host.ChoiceDescription "&No", "Don't Push"
$options1 = [System.Management.Automation.Host.ChoiceDescription[]]($yes1, $no1)
$choiceRTN1 = $host.ui.PromptForChoice($caption1,$message1, $options1,$defaultChoice1)
if ( $choiceRTN1 -ne 1 )
{
& docker push registry.heroku.com/$appname/web
}
else
{
pause
exit
}






#Confirmation dialogue
$caption2 = "Please Confirm"    
$message2 = "Release Image in heroku:"
[int]$defaultChoice2 = 0
$yes2 = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes", "release."
$no2 = New-Object System.Management.Automation.Host.ChoiceDescription "&No", "Do not release."
$options2 = [System.Management.Automation.Host.ChoiceDescription[]]($yes2, $no2)
$choiceRTN2 = $host.ui.PromptForChoice($caption2,$message2, $options2,$defaultChoice2)
if ( $choiceRTN2 -ne 1 )
{& heroku container:release web --app $appname}
else
{Write-host 'cancelled releas'}
echo 'all done'
pause