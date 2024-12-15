# Decourrupt a Repo into it's remote stage (All changes will be lost)

```cmd
@echo off
del .git\refs\heads\master
del .git\refs\remotes\origin\master
del .git\refs\remotes\origin\HEAD
del .git\logs\HEAD
del .git\logs\refs\heads\master
del .git\logs\refs\remotes\origin\HEAD
del .git\logs\refs\remotes\origin\master
git fetch origin
git reset --hard origin/master
echo Repository has been de-corrupted and synced with remote.
pause

```
