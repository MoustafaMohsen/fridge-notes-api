echo "------------------ Current Status ------------------"
git status
$commit = Read-Host 'Please enter commit message'
echo "------------------ Staging Files------------------"
git add *
echo "------------------ Commiting ------------------"
& git commit -am $commit
echo "------------------ Pushing ------------------"
git push
pause