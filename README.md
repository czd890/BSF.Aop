#BSF.Aop#
.Net **免费开源，静态Aop织入**（直接修改IL中间语言）框架，类似PostSharp(收费)；<br/>实现前后Aop切面和INotifyPropertyChanged注入方式。<br/><br/>
描述：<br/>

- 项目结构扩展自BSF基础架构，但dll方面并不依赖BSF.dll，可以独立开发使用。<br/>
- 项目代码量少（也就几个类文件），核心功能插件化开发，便于第三方扩展，阅读，调试，一起来完善。<br/>
- .net Aop静态织入相关的免费开源项目比较少或暂未听闻，故希望开源这块内容，成为同类型开源项目的起点和借鉴。<br/>

by 车江毅 

# 使用配置 #
1）项目引用BSF.Aop.dll,引用Mono.Cecil.dll，引用Mono.Cecil.Pdb.dll。<br/>
2）项目启动代码添加AopStartLoader.Start();一句代码即可。<br/>
（该代码用于自动注入扫描和vs项目环境自动配置,导出相关exe文件等）<br/>

备注：

- Web项目在Application_Start中添加<br/>
- Winform项目在Pragram.Main中添加<br/>


### 自动化配置（默认推荐） ###
Build项目，然后直接运行调试项目。<br/>

备注：

- AopStartLoader.Start();这句代码第一次项目运行会报错，因为Aop环境未曾自动搭建；第一次运行并自动搭建成功后，以后运行都是自动注入的。<br/>
- 默认会在项目下生成“packages.BSF.Aop”文件夹,并在vs项目PostBuildEvent事件中注入“aop运行脚本”，具体参考以下“手工配置内容”。<br/>

### 手工配置 （遇到问题，推荐使用） ###
1）配置Aop注入目录。<br/>
vs项目下新建packages.BSF.Aop 目录，里面分别包含BSF.Aop.ILRun.exe，BSF.Aop.dll，Mono.Cecil.Pdb.dll,Mono.Cecil.dll 这几个文件。<br/>
2）配置PostBuildEvent 脚本。<br/>

#### winform环境: ####
打开vs-》项目属性-》PostBuildEvent，配置aop运行脚本。如：<br/>
xcopy $(OutDir)BSF.Aop.dll $(ProjectDir)packages.BSF.Aop\ /Y<br/>
call "$(ProjectDir)packages.BSF.Aop\BSF.Aop.ILRun.exe" msgbox $(TargetDir)<br/>


#### web环境: ####
打开vs-》项目属性-》PostBuildEvent，配置aop运行脚本。(OpenApi.Test.Web替换成具体的项目名) 如：<br/>
xcopy $(SolutionDir)\OpenApi.Test.Web\bin\BSF.Aop.dll $(SolutionDir)\OpenApi.Test.Web\packages.BSF.Aop\ /Y<br/>
call "$(SolutionDir)\OpenApi.Test.Web\packages.BSF.Aop\BSF.Aop.ILRun.exe" msgbox $(SolutionDir)\OpenApi.Test.Web\bin\<br/>


# 使用demo示例 #
1) 前后Aop切面示例 （*详细参考BSF.Aop.Test项目*）
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

2) INotifyPropertyChanged 示例（暂未测试真正使用效果,*详细参考BSF.Aop.Test项目* ）
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
