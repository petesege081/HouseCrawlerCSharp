@echo off  
echo Chrome
tasklist /FI "IMAGENAME eq chromedriver.exe"
echo.
echo Edge
tasklist /FI "IMAGENAME eq msedgedriver.exe"
echo.
echo Firefox
tasklist /FI "IMAGENAME eq geckodriver.exe"
echo.
pause