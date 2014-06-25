@echo Off

sc create Schedule.Tasks.Host binPath= %cd%\Schedule.Tasks.Host.WindowsService.exe start= auto

net start Schedule.Tasks.Host

echo 安装完成。（按任意键退出）

Pause