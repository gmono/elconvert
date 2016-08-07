using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class DllAnaly:ITrans
    {
        bool mrawpname;//是否使用原始变量名
        string mfrontstr; //文件前缀
        string mzsfmstr;  //注释格式化字符串
        string mvarfmstr;//变量注释格式化字符串
        string mcallway;//调用约定
        /// <summary>
        /// 初始化DLL分析器
        /// </summary>
        /// <param name="rawpname">是否采用原变量名，false将采用自动生成变量名</param>
        /// <param name="frontstr">文件前缀文本</param>
        /// <param name="zsfmstr">注释格式字符串 0代表一行注释</param>
        /// <param name="varfmstr">变量注释格式化字符串</param>
        /// <param name="callway">调用约定字符串</param>
        public DllAnaly(bool rawpname=false,string frontstr="",string zsfmstr="///{0}",string varfmstr="///{0} {1}",string callway="__stdcall")
        {
            mrawpname = rawpname;
            mfrontstr = frontstr;
            mzsfmstr = zsfmstr;
            mvarfmstr = varfmstr;
            mcallway = callway;
        }
        class PortDesc
        {
            //暂时支持文本和整数以及bool值
            public string funname = "";//函数名
            public ParType rettype = ParType.Int;//返回类型
            public class Par
            {
                public ParType type = ParType.Int;//参数类型
                public bool isdizhi = false;//是否传地址
                public string name = null;//参数名
                public string beizhu = null;//备注
            }
            public IEnumerable<Par> pars = null;//参数表
            public string beizhu = null;//函数备注

        }
        string createCode(PortDesc desc, int index)
        {
            //生成注释部分
            //每填充一行加一个换行 结果最后刚好╮(╯_╰)╭
            StringBuilder ret = new StringBuilder("");
            if (desc.beizhu != null) ret.Append(string.Format(mzsfmstr+"\r\n", desc.beizhu));
            if (desc.pars != null)
                foreach (var v in desc.pars)
                {
                    if (v.beizhu != null)
                    {
                        //用不同的格式化字符串格式化变量和函数备注
                        ret.Append(string.Format(mvarfmstr+"\r\n", v.name, v.beizhu));
                    }
                }


            //
            
            string[] types = Common.types;//得到类型文本
            ret.Append(types[(int)desc.rettype]);//明确值的枚举当整数，方便 放入类型文本
            ret.Append(" ");
            //ret.Append(mcallway);//加上调用约定字符串
            //ret.Append(' ');//得加上空格以保证正确
            //上面搞错了 本来不是用在这里的
            ret.Append(desc.funname);
            ret.Append("(");

            bool isone = true;
            StringBuilder partab = new StringBuilder(""); //不带括号的参数表 如 int a,bool b
            if (desc.pars != null)
            {
                foreach (var v in desc.pars)
                {
                    if (isone) isone = false;
                    else partab.Append(',');//如果是后面的参数就要加逗号
                                            //生成参数表
                    partab.Append(types[(int)v.type]);
                    partab.Append(' ');
                    if (v.isdizhi)
                    {
                        partab.Append('*');//指针
                    }
                    partab.Append(v.name);//变量名
                }
            }
            ret.Append(partab);
            ret.Append(")\r\n{\r\n");//结束括号以及函数体开头
            if (desc.rettype != ParType.None) ret.Append("return ");//如果有返回值就加return
            string typestr = string.Format(types[(int)desc.rettype] + " ({0} *)({1})",mcallway, partab);
            StringBuilder tstr = new StringBuilder("");//这个是调用参数表 如 a,b,c,d
            if (desc.pars != null)
            {
                isone = true;//初始化判断标记变量
                foreach (var s in desc.pars)
                {
                    if (isone) isone = false;
                    else tstr.Append(',');//如果是后面的参数就要加逗号
                    tstr.Append(s.name);//加上其名字
                }
            }
            ret.Append(string.Format("(({0})_dll__funs[{1}])({2});\r\n", typestr, index, tstr));
            ret.Append("}\r\n\r\n");//看来要放到这里
            return ret.ToString();
        }


        class dllport
        {
            public string dllname = "";//dll文件名
            public string dllp = "";//dll导出函数名
        }

        string createPortGet(IEnumerable<dllport> ports)
        {
            //dpls_init_load all port
            string ret = "void DPLS_INIT_LOADALLPORT(){\r\nvoid *hel=NULL;\r\nvoid *tempp=nullptr;\r\n";//函数头
            SortedSet<string> dllset = new SortedSet<string>();
            int i = 0;//计数器
            foreach (var v in ports)
            {
                if (!dllset.Contains(v.dllname))
                {
                    //如果这个dll还没有加载
                    ret += string.Format("hel=LoadLibrary({0});\r\n", v.dllname);
                    dllset.Add(v.dllname);//加入，表示已经加载

                }
                ret += string.Format("tempp = GetProcAddress(hel,{0});\r\n_dll__funs[{1}]=tempp;\r\n",
                    v.dllp, i);//搜索并获得函数指针,如果加载不成功程序卡死 获得地址不成功就是nullptr

                ++i;
            }
            ret += "}";
            return ret;
        }
        public string trans(string str)
        {
            //如果不是易语言文本就返回空
            if (str.Substring(0, 5) != ".版本 2")
                return "";
            string dlls = str.Substring(5);//从5处开始得到dll接口序列

            string[] lines = dlls.Split('\n');//分隔出多行
            PortDesc desc = null;//当前处理的函数

            string codes = "";//返回的代码

            List<PortDesc.Par> tpars = new List<PortDesc.Par>();//临时用的参数表
            List<dllport> dlps = new List<dllport>();//dll的导出接口名字表 用于给导出代码生成函数生成函数指针获取代码

            //函数数量计数
            int count = 0;

            //由于无法采用中文变量和参数名
            //所以暂时使用自动生成参数名的方法
            IncreaseStr varnames = new IncreaseStr("__par{0}", 0);
            foreach (var v in lines)
            {
                //对每一行处理
                string pure = v.Trim();
                if (pure.Length == 0) continue;//跳过空行
                if (pure[0] == '.')
                {
                    if (pure.Substring(1, 2) == "参数")
                    {

                        //读参数状态
                        if (desc == null) throw new Exception("读取发生错误！");
                        string partab = pure.Substring(3);
                        string[] par = partab.Split(',');//逗号分隔
                        PortDesc.Par p = new PortDesc.Par();//生成临时参数变量
                        for (int t = 0; t < par.Length; ++t)
                        {
                            string purename = par[t].Trim();//得到名字
                            switch (t)
                            {
                                case 0:
                                    //name是参数名
                                    //判断是不是采用原变量名
                                    if (mrawpname) p.name = purename;
                                    else
                                    {

                                        p.name = varnames.Next();
                                    }
                                    break;
                                case 1:
                                    //name是类型名
                                    p.type = Common.GetParType(purename);
                                    if (p.type == ParType.None) throw new Exception("变量类型错误！！");
                                    break;
                                case 2:
                                    //name是属性名
                                    //目前只处理传址 数组我还不知道是什么意思
                                    switch (purename.Length)
                                    {
                                        case 0:
                                            //属性为空
                                            p.isdizhi = false;
                                            break;
                                        case 2:
                                            //有一个属性
                                            if (purename == "传址" && p.type != ParType.String && p.type != ParType.ByteSet)
                                            {
                                                p.isdizhi = true;
                                            }
                                            else p.isdizhi = false;
                                            break;
                                        case 5:
                                            //有两个属性 注意两个属性中间有个空格
                                            //这里暂时没有判断 也就是说如果是文本 字节集之类的就会是双重指针 
                                            p.isdizhi = true;//只处理这个属性
                                            break;
                                    }
                                    break;
                                case 3:
                                    //name是备注
                                    p.beizhu = purename;//保存备注
                                    break;

                            }
                        }
                        tpars.Add(p);//放入临时参数表

                    }
                    else
                    {
                        //如果不是参数那就是dll接口声明 默认如此 有问题那就出错好了
                        //上面是脑子抽了用了for循环 TMDGBD
                        //还要 是英文逗号，不是中文
                        if (desc != null)
                        {
                            //目前正在处理函数
                            //开启一个新函数
                            //放入参数表
                            desc.pars = tpars;
                            tpars = new List<PortDesc.Par>();//生成一个新的临时参数表
                            codes += createCode(desc, count);//加入代码
                            count++;//
                            //
                            varnames = new IncreaseStr("__par{0}", 0);
                            desc = new PortDesc();//生成一个新函数
                        }
                        else desc = new PortDesc();//第一个函数

                        string prostr = v.Substring(6);//取得后面的属性表文本
                        string[] pros = prostr.Split(',');
                        for (int s = 0; s < pros.Length; ++s) pros[s] = pros[s].Trim();//直接纯净化
                        desc.funname = pros[0];//第一个是函数名
                        //第二个是函数返回值类型
                        desc.rettype = Common.GetParType(pros[1]);
                        //第三第四个分别是 dll文件和导出接口
                        dllport dp = new dllport();
                        dp.dllname = pros[2];
                        dp.dllp = pros[3];//设置信息
                        dlps.Add(dp);//放入表中
                        //第五个是公开 忽略
                        //第六个是备注,可能没有
                        if (pros.Length == 6)
                            desc.beizhu = pros[5];
                        //要是没有就忽略过去
                    }
                }


            }
            //循环结束 而 必有最后一个函数加进去 也就是目前的desc

            codes += createCode(desc, count);//加进去,同样注意1和0的差异
            count++;

            string ret = mfrontstr;//加上文件头
            ret +=string.Format("\r\n\r\n#include <windows.h>\r\n\r\nvoid *_dll__funs[{0}];\r\n", count);//加上函数指针数组头
            ret += codes;
            ret += createPortGet(dlps);//加入接口获取代码
            return ret;
        }
    }
}
