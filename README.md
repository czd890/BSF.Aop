#BSF.Aop#
.net **静态Aop织入**（直接修改IL中间语言），类似PostSharp，但是免费开源。 实现前后Aop切面和INotifyPropertyChanged注入方式。<br/><br/>
备注：目前.net Aop静态织入相关的免费开源项目暂未听闻，故希望开源这块内容，成为同类型开源项目的起点和参考。<br/>

# 使用配置 #
1）项目引用BSF.Aop.dll,引用Mono.Cecil.dll，引用Mono.Cecil.Pdb.dll。<br/>
2）项目启动代码添加AopStartLoader.Start();一句代码即可。<br/>
（该代码用于自动注入扫描和vs项目环境自动配置,导出相关exe文件等）<br/>
备注：Web项目在Application_Start中添加，Winform项目在Pragram.Main函数添加。<br/>


### 自动化配置（默认推荐） ###
运行项目。<br/>
（AopStartLoader.Start();第一次运行会报错，因为Aop环境会自动搭建，自动搭建成功后，以后运行都是自动注入的。）<br/>
（默认会在项目下生成“packages.BSF.Aop”,vs项目PostBuildEvent事件中注入“aop运行脚本”，参考手工配置内容）<br/>

### 手工配置 （假如遇到问题，推荐使用） ###
1）配置Aop注入目录。<br/>
vs项目下新建packages.BSF.Aop 目录，里面分别包含BSF.Aop.ILRun.exe，BSF.Aop.dll，<br/>Mono.Cecil.Pdb.dll,Mono.Cecil.dll 这几个文件。<br/>
2）配置PostBuildEvent 脚本。<br/>

#### winform环境: ####
打开vs-》项目属性-》PostBuildEvent，配置aop运行脚本。如：<br/>
xcopy $(OutDir)BSF.Aop.dll $(ProjectDir)packages.BSF.Aop\ /Y<br/>
call "$(ProjectDir)packages.BSF.Aop\BSF.Aop.ILRun.exe" msgbox $(TargetDir)<br/>


#### web环境: ####
打开vs-》项目属性-》PostBuildEvent，配置aop运行脚本。(OpenApi.Test.Web替换成具体的项目名) 如：<br/>
xcopy $(SolutionDir)\OpenApi.Test.Web\bin\BSF.Aop.dll $(SolutionDir)\OpenApi.Test.Web<br/>\packages.BSF.Aop\ /Y<br/>
call "$(SolutionDir)\OpenApi.Test.Web\packages.BSF.Aop\BSF.Aop.ILRun.exe" msgbox $(SolutionDir)\OpenApi.Test.Web\bin\<br/>


# 使用demo示例 #
1) 前后Aop切面示例
<pre class="brush:c#;toolbar: true; auto-links: false;">
public class AroundAopTest
    {
        [MyAroundAop]
        [AttributeInfo(Des = "测试2")]
        public void Method(TempInfo info, out int b,int a=1)
        {
            a = 222;
            b = 3;
            System.Console.WriteLine("Hello world!"+a);
        }
    }

    public static class AroundAopTest2
    {
        [MyAroundAop][AttributeInfo(Des ="测试")]
        public static void Method2(TempInfo info, int a = 1)
        {
            a = 222;
            System.Console.WriteLine("Hello world!" + a);

        }
    }

    public class MyAroundAop : Aop.Attributes.Around.AroundAopAttribute
    {
        public MyAroundAop()
        {
        }


        public override void Before(AroundInfo info)
        {
            var att = info.Method.CustomAttributes.ToList()[0];
            info.Params["a"] = 55;
            System.Console.WriteLine("before" + info.Params["a"]);
        }

        public override void After(AroundInfo info)
        {
            System.Console.WriteLine("after"+ info.Params["a"]);
        }
    }

    public class TempInfo
    {
        public int T1 { get; set; }
    }

    public class AttributeInfo : System.Attribute
    {
        public string Des { get; set; }
    }
</pre> 

2) INotifyPropertyChanged 示例（暂未测试真正使用效果）
<pre class="brush:c#;toolbar: true; auto-links: false;">

 [NotifyPropertyChangedAop]
    public class User
    {
        public string Name { get; set; }

        public int Age { get; set; }

        [NoAop]
        public int B { get; set; }
    }
</pre> 
