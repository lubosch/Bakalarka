using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serves_skuska
{
    class Labl : System.Windows.Forms.Label
    {
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {

                if (base.InvokeRequired)
                {
                    base.Invoke(new System.Windows.Forms.MethodInvoker(delegate { base.Text = value; }));
                }
                else
                {
                    base.Text = value;
                }

            }
        }
    }
}
