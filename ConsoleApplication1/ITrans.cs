using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{

    interface ITrans
    {
        /// <summary>
        /// 获取转换后的文本
        /// </summary>
        /// <param name="str">原文本</param>
        /// <returns></returns>
        string trans(string str);

    }
}
