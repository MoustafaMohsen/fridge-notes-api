#Name Mohesen 2018
#HardCoded Variables
$appname = "linux-docker-5"
$imagename = "fridge_api_1"
$publishdirectory= ".\bin\docker\aspcore\"


$input1 = Read-Host 'Change appname?(default is:"'$appname'")'
$input2 = Read-Host 'Change imagename?(default is:"'$imagename'")'
$input3 = Read-Host 'Change publishdirectory?(default is:"'$publishdirectory'")'

if ( ![string]::IsNullOrWhitespace($input1)  )
{
$appname = $input1
}
else
{
}

if ( ![string]::IsNullOrWhitespace($input2)  )
{
$imagename = $input2
}
else
{
}

if ( ![string]::IsNullOrWhitespace($input3)  )
{
$publishdirectory = $input3
}
else
{
}


heroku container:login

dotnet publish -c Release -o $publishdirectory

Copy-Item ".\Dockerfile" -Destination $publishdirectory

#Confirmation dialogue
$caption0 = "build Image"    
$message0 = ''

[int]$defaultChoice0 = 0
$yes0 = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes", "Push"
$no0 = New-Object System.Management.Automation.Host.ChoiceDescription "&No", "Don't Push"
$options0 = [System.Management.Automation.Host.ChoiceDescription[]]($yes0, $no0)
$choiceRTN0 = $host.ui.PromptForChoice($caption0,$message0, $options0,$defaultChoice0)
if ( $choiceRTN0 -ne 1 )
{
& docker build -t $imagename $publishdirectory
}
else
{
pause
exit
}
echo '
========================================
Continue to tag image ?'

pause

echo '

start image tagging'

& docker tag $imagename registry.heroku.com/$appname/web
& docker images registry.heroku.com/$appname/web

echo '
end image tagging
'


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