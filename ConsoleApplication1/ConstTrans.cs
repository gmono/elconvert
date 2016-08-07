using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class ConstTrans : ITrans
    {
        string descstr = "";
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="d">说明文本</param>
        public ConstTrans(string d)
        {
            descstr = d;
        }
        class ConstData
        {
            public string name=null;
            public ParType type=ParType.None;//常量类型
            public string value=null;//值，值都是字符串类型 而易语言中的字符串常量则是在内部还有个中文双引号
            public string beizhu=null;//备注 不用多说了
        }
        /// <summary>
        /// 根据值的文本判断值的类型
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        ParType getType(string value)
        {
            if (value[0] == '“') return ParType.String;//注意是中文双引号的左边
            if (value.Contains(".")) return ParType.Double;//有小数点就是小数
            if (value[0] == '真' || value[0] == '假') return ParType.Bool;//真假都是bool 
            if (value[0] == '<') return ParType.None;//此处的None指的是长文本常量时的情况
            return ParType.Int;//不保证是int不过如果不是的话 第一 是其他不能处理的类型 正好出错
            //2 是错误的文本 最好出错
        }
        string createCode(ConstData data)
        {
            //注意 只生成两行 最后面不包含换行符
            StringBuilder ret =new StringBuilder();
            if(data.beizhu!=null&&data.beizhu.Length>0) ret.AppendFormat("//{0}\r\n", data.beizhu);//加备注
            //类型相关 主要处理长文本类型
            string typestr = "";
            string typeval = "";
            if (data.type == ParType.None)
            {
                typestr = Common.types[(int)ParType.String];
                typeval =string.Format("\"{0}\"", data.value);//和文本最大的不同是没有双引号,不做作为字符串应该有双引号
            }
            else
            {
                typestr = Common.types[(int)data.type];
                switch(data.type)
                {
                    case ParType.Bool:
                        if (data.value == "真") typeval = "true";
                        else typeval = "false";
                        break;
                    case ParType.Double:
                        typeval = data.value;
                        break;
                    case ParType.Int:
                        typeval = data.value;
                        break;
                    case ParType.String:
                        if (data.value.Length > 2) typeval = string.Format("\"{0}\"",data.value.Substring(1, data.value.Length - 2));//得到真实文本 并且变成字符串形式
                        else typeval = "\"\"";//如果是空文本
                        break;
                    default:
                        throw new Exception("未知的类型！！");
                            
                }

            }
            ret.AppendFormat("const {0} {1}={2};\r\n\r\n", typestr, data.name, typeval);//为了看起来
            //为了看起来清晰所以后面加了两个换行 本来原则上是这个函数生成一行的不过既然有备注了这个原则就没用了……
            return ret.ToString();
            
        }
        public string trans(string str)
        {
            if (str.Substring(0, 5) != ".版本 2") throw new Exception("输入错误！！！");
            string[] lines = str.Substring(5).Split('\n');
            StringBuilder builder = new StringBuilder();
            foreach(var v in lines)
            {
                string s = v.Trim();
                if (s.Length == 0) continue;
                if (s.Substring(0, 3) != ".常量") throw new Exception("输入错误！！！！");
                string[] pros = s.Substring(3).Split(',');
                for (int i = 0; i < pros.Length; i++) pros[i] = pros[i].Trim();
                //至少有两个成员 否则就让其出错去吧……
                ConstData data = new ConstData();
                data.name = pros[0];
                string val = pros[1].Substring(1, pros[1].Length - 2);//得到双引号中间的文本
                data.type = getType(val);
                data.value = val;
                //第三个 是否公开但是我不管这卵东西
                //第四个是备注 如果有的话 必定是第四个 第三个会是空格
                if(pros.Length==4)
                {
                    //备注里都是中文符号 而代码里的逗号是英文的
                    //设计易语言的人也是机智……
                    data.beizhu = pros[3];
                }
                string line = createCode(data);
                builder.Append(line);//添加一行 一个常量
                builder.Append("\r\n");//换行
            }

            return descstr+"\r\n\r\n"+builder.ToString();
        }
    }
}
