#BSF.Aop#
.net 静态Aop织入（直接修改IL中间语言），类似PostSharp，但是免费开源。 实现前后Aop切面和INotifyPropertyChanged注入方式。<br/>
备注：目前.net Aop静态织入相关的开源项目暂未听闻。<br/>

# 引用配置 #
1.项目引用BSF.Aop.dll,引用Mono.Cecil.dll，引用Mono.Cecil.Pdb.dll。<br/>
1.1 
项目启动代码添加AopStartLoader.Start();一句代码即可。<br/>
（该代码用于自动注入扫描和vs项目环境自动配置,导出相关exe文件等）<br/>
备注：Web项目在Application_Start中添加，Winform项目在Pragram.Main函数添加。<br/>


# 自动化配置示例（默认推荐 #
2.运行项目。（AopStartLoader.Start();第一次运行会报错，因为Aop环境会自动搭建，自动搭建成功后，以后运行都是自动注入的。）<br/>
（默认会在项目下生成“packages.BSF.Aop”,vs项目PostBuildEvent事件中注入“aop运行脚本”，参考手工配置内容）<br/>

# 手工配置 （出问题后，推荐使用） #
3.1 配置Aop注入目录。<br/>
配置 packages.BSF.Aop 目录，里面分别包含BSF.Aop.ILRun.exe，BSF.Aop.dll，<br/>Mono.Cecil.Pdb.dll,Mono.Cecil.dll 这几个文件。<br/>
3.2 配置PostBuildEvent 脚本。<br/>

## winform环境: ##
打开vs-》项目属性-》PostBuildEvent，配置aop运行脚本。如：<br/>
xcopy $(OutDir)BSF.Aop.dll $(ProjectDir)packages.BSF.Aop\ /Y<br/>
call "$(ProjectDir)packages.BSF.Aop\BSF.Aop.ILRun.exe" msgbox $(TargetDir)<br/>


## web环境:(OpenApi.Test.Web替换成具体的项目名) ##
xcopy $(SolutionDir)\OpenApi.Test.Web\bin\BSF.Aop.dll $(SolutionDir)\OpenApi.Test.Web<br/>\packages.BSF.Aop\ /Y<br/>
call "$(SolutionDir)\OpenApi.Test.Web\packages.BSF.Aop\BSF.Aop.ILRun.exe" msgbox $(SolutionDir)\OpenApi.Test.Web\bin\<br/>

