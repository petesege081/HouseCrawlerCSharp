@echo off
echo Chrome
taskkill /f /im chromedriver.exe
echo.
echo Edge
taskkill /f /im msedgedriver.exe
echo.
echo FireFox
taskkill /f /im geckodriver.exe
echo.
pause