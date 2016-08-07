using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace ConsoleApplication1
{
    class Program
    {


        //
        //-----------------------------------------------------------------------------------
        //以下代码无需改动,改动出问题后果自负

        static void Main(string[] args)
        {
            Console.WriteLine("易语言DLL声明转换程序\n");
            Console.WriteLine("转换模块初始化中……");
            Common.InitAnalyObjs();
            Console.WriteLine("初始化完毕");
            Console.WriteLine("请注意文件必须是纯粹的从易语言中复制出来的文本，并且前面不能有空，否则不能识别");
            Console.WriteLine("注意文件的文件名和路径不能有空格 否则不能识别");
            open:
            
            Console.Write(Common.descstr);
            Console.Write("\n请输入要执行的操作：");
            char doid = Console.ReadLine()[0];//注意读出来的是字符,要读一行不然下面会读到空行
            ITrans traobj = null;
            if (doid == '0') return;//退出
            else if (doid-'0' > Common.transobjs.Length)
            {
                Console.WriteLine("输入错误！！");
                goto open;
            }
            else traobj = Common.transobjs[doid - '0' - 1];//从字符到数字再到索引
            if(traobj==null)
            {
                Console.WriteLine("暂时还没实现……╮(╯_╰)╭");
                goto open;
            }

            //开始
            Console.Write("\n请输入要转换的声明文本：");
            string strfile = Console.ReadLine();
            FileInfo strf = new FileInfo(strfile);
            if (!strf.Exists) { Console.WriteLine("文件不存在"); goto open; }

            string str = File.ReadAllText(strfile);


            Console.Write("\n请输入保存的文件名:");
            string file = Console.ReadLine();
            //FileInfo innfo = new FileInfo(file);
            //if (innfo.Exists) { Console.WriteLine("文件已经存在"); goto save; }
            //目前允许覆盖并且不提示

            
            string codes = "";
            //下面注释主要是捕捉了错误 就不好调试了，干脆不捕捉
            /* try
             {*/

                    
                    codes = traobj.trans(str);
                    

                /*
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
                return;
            }*/
            //save:

            File.WriteAllText(file, codes);
            goto open;

        }

    }
}
