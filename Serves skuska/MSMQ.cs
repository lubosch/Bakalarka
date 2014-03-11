using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;
using System.Runtime.InteropServices;


namespace Serves_skuska
{

    class MSMQ
    {
        Labl stav;
        private volatile bool done;
        Labl sprava;
        Labl odoslane;
        Labl chyby;


        public MSMQ(Labl stav, Labl sprava, Labl chyby, Labl odoslane)
        {
            this.sprava = sprava;
            this.stav = stav;
            this.chyby = chyby;
            this.odoslane = odoslane;
        }

        public void SendMessageToQueue(string msg, [Optional, DefaultParameterValue(@".\private$\server_skuska")]string queueName)
        {

            // check if queue exists, if not create it
            MessageQueue msMq = null;
            //try
            //{
            //    MessageQueue.Exists(queueName);
            //}
            //catch (InvalidOperationException ex)
            //{
            //    chyby.Text=(ex.ToString());
            //}

            


            if (!MessageQueue.Exists(queueName))
            {
                msMq = MessageQueue.Create(queueName);
            }
            else
            {
                msMq = new MessageQueue(queueName);
            }

            try
            {
                // msMq.Send("Sending data to MSMQ at " + DateTime.Now.ToString());
                msMq.Send(msg);
                odoslane.Text = msg;
            }
            catch (MessageQueueException ee)
            {
                chyby.Text = ee.ToString();
            }
            catch (Exception eee)
            {
                chyby.Text = eee.ToString();
            }
            finally
            {
                msMq.Close();
            }
            //Console.WriteLine("Message sent ......");

        }

        public void ReceiveMessageFromQueue([Optional, DefaultParameterValue(@".\private$\server_skuska")]string queueName)
        {
            MessageQueue msMq = msMq = new MessageQueue(queueName);
            try
            {
                 msMq.Formatter = new XmlMessageFormatter(new Type[] {typeof(string)});
                //msMq.Formatter = new XmlMessageFormatter(new Type[] { typeof(Person) });
                var message = (string)msMq.Receive().Body;
                //Console.WriteLine("FirstName: " + message.FirstName + ", LastName: " + message.LastName);
                sprava.Text = message;
                stav.Text = ("Prijata sprava");
                msMq.Close();
                spracuj (message);
                return;
            }
            catch (MessageQueueException ee)
            {
                chyby.Text=(ee.ToString());
            }
            catch (Exception eee)
            {
                chyby.Text=(eee.ToString());
            }
            finally
            {
                msMq.Close();
            }
            spracuj( "CHYBA");
        }

        private void spracuj(string msg)
        {

        }
    }
}
