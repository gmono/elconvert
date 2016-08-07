using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public enum ParType { String = 0, Int = 1, Bool = 2, None = 3,ByteSet=4,Double=5 }//设置明确值便于内部寻址
    
    class Common
    {

        //以下代码可以随着需要更改
        /// <summary>
        /// 功能说明文本
        /// </summary>
        public static string descstr = "\n1 :DLL接口生成\t 2 :常量定义生成\n 0:退出";
        /// <summary>
        /// 初始化转换器对象数组
        /// </summary>
        public static void InitAnalyObjs()
        {
            transobjs = new ITrans[2];//暂时放两个 一个没写
            //第一个是自动变量名 以为中文变量名的注释容易弄混 所以注释格式化字符串不一样 加了双引号
            transobjs[0] = new DllAnaly(false, "//月落专用易语言dll接口声明转换器生成\r\n//QQ:973544732", "/// \\brief {0}", "/// \\param {0}  {1}");
            //transobjs[0] = new DllAnaly(true, "//月落专用易语言dll接口声明转换器生成\r\n//QQ:973544732", "/// \\brief {0}", "/// \\param {0}\t\"{1}\"");
            transobjs[1] = new ConstTrans("//月落专用易语言常量声明转换器生成\r\n//QQ:973544732");
        }


        //
        //------------------------------------------------------------
        //以下代码不可随意更改 更改后果自负
        //除了添加类型支持时可以更改外都不要更改

        public static ITrans[] transobjs = null;

        public static ParType GetParType(string name)
        {
            switch (name)
            {
                case "文本型":
                    return ParType.String;
                case "整数型":
                    return ParType.Int;
                case "逻辑型":
                    return ParType.Bool;
                case "字节集":
                    return ParType.ByteSet;
                case "双精度小数型":
                    return ParType.Double;
                case "":
                    //没有返回值
                    return ParType.None;
            }
            throw new Exception("类型解析错误！！");
        }
        public static string[] types = { "char *", "int", "bool", "void", "unsigned char *", "double" };//对应的类型文本

    }
    class IncreaseStr
    {
        int now;
        string mfmt;
        public IncreaseStr(string format, int start)
        {
            now = start;
            mfmt = format;
        }
        public string Next()
        {

            return string.Format(mfmt, now++);

        }
    }

}
