using MUD_Skeleton.Commons.Auxiliary;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MUD_Skeleton.Commons.Comms
{
    public enum StatusMessage { Error = -2, NonRelevantUsage = -1, ReadyToSend = 0, Delivered = 1, Executed = 2 }

    public class Message
    {
        #region Static Class Operation Required
        public static ConcurrentDictionary<string, Pares<DateTime, Message>> dic_ActiveMessages = new ConcurrentDictionary<string, Pares<DateTime, Message>>();

        public static ConcurrentDictionary<string, Pares<DateTime, Message>> dic_BackUpMessages = new ConcurrentDictionary<string, Pares<DateTime, Message>>();
        #endregion

        #region Attributes
        #region Basic
        [JsonInclude]
        public string text = string.Empty;
        [JsonIgnore]
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    text = value;
                    return;
                }
                text = UtilityAssistant.Base64Encode(value);
                Length = (uint)text.Length;
            }
        }
        [JsonIgnore]
        public string TextOriginal
        {
            get
            {
                string a = text;
                try
                {
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }

                    string resultTryDecode = string.Empty;
                    if (UtilityAssistant.TryBase64Decode(a, out resultTryDecode))
                    {
                        a = resultTryDecode;
                    }

                    return a;
                }
                catch (Exception ex)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine("\n\nError Property TextOriginal Message: " + ex.Message);
                    Console.ResetColor();
                    return a;
                }
            }
            private set
            {
                text = value;
            }
        }

        private uint idMsg = 0;
        public uint IdMsg { get => idMsg; set => idMsg = value; }

        private uint idChl = 0;
        public uint IdChl
        {
            get
            {
                return idChl;
            }
            set
            {
                idChl = value;
                if (idChl != 0)
                {
                    Status = StatusMessage.ReadyToSend;
                }
            }
        }

        private uint idSnd = 0;
        public uint IdSnd
        {
            get
            {
                return idSnd;
            }
            set
            {
                idSnd = value;
            }
        }

        private uint length = 0;
        public uint Length { get => length; private set => length = value; }

        public StatusMessage Status = StatusMessage.NonRelevantUsage;
        #endregion

        #region functionals
        private bool IsBlockMultiMessage = false;
        #endregion
        #endregion

        #region Constructors
        #region Natural Constructors
        public Message()
        {
            IdMsg = 0;
            Text = string.Empty;
            IdChl = 0;
            Status = StatusMessage.NonRelevantUsage;
        }

        public Message(string text)
        {
            IdMsg = 0;
            Text = text;
            IdChl = 0;
            Status = StatusMessage.NonRelevantUsage;
        }

        public Message(uint idOfChl, string text)
        {
            IdMsg = 0;
            Text = text;
            IdChl = idOfChl;
            Status = StatusMessage.NonRelevantUsage;
        }
        #endregion

        #region Artificial Methods of Construction
        public static Message CreateMessage(string text, bool BlockChainOfMessages = false, uint idForRef = 0)
        {
            try
            {
                Message msg = new Message();
                if (BlockChainOfMessages)
                {
                    msg.IsBlockMultiMessage = true; //it start false, therefore is unnecesarrely to change it if somebody add it as a false
                }
                string marker = string.Empty;
                if (text.Length >= 4)
                {
                    if (text.Substring(0, 4).Contains(":"))
                    {
                        marker = text.Substring(0, text.IndexOf(":") + 1);
                        text = text.ReplaceFirst(marker, "");
                    }
                }
                string jsonAProc = UtilityAssistant.CleanJSON(text);
                if (!string.IsNullOrWhiteSpace(marker))
                {
                    jsonAProc = marker + jsonAProc;
                }
                msg.Text = jsonAProc;
                if (idForRef != 0)
                {
                    msg.idChl = idForRef;
                }
                return msg;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Message CreateMessage: " + ex.Message);
                return new Message();
            }
        }
        #endregion
        #endregion

        #region Supplementary Methods
        public static bool ValidTextFromJsonMsg(string jsonText)
        {
            try
            {
                if (jsonText.Contains("text"))
                {
                    string base64text = jsonText.Substring(jsonText.IndexOf("text"));
                    base64text = base64text.Substring(base64text.IndexOf(":") + 1);
                    base64text = base64text.Substring(0, base64text.LastIndexOf("\"")).Replace("\"", "");
                    string result = string.Empty;
                    return UtilityAssistant.TryBase64Decode(base64text, out result);
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error static bool ValidTextFromJsonMsg: " + ex.Message);
                return false;
            }
        }

        public static bool IsValidMessage(string jsonMsg)
        {
            try
            {
                if (!jsonMsg.Contains("{"))
                {
                    return false;
                }
                if (!jsonMsg.Contains("Length"))
                {
                    return false;
                }
                /*if (!jsonMsg.Contains("IdRef"))
                {
                    return false;
                }*/
                if (!jsonMsg.Contains("IdChl"))
                {
                    return false;
                }
                if (!jsonMsg.Contains("IdMsg"))
                {
                    return false;
                }
                if (!jsonMsg.Contains("text"))
                {
                    return false;
                }
                if (!jsonMsg.Contains("}"))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error static bool ValidTextFromJsonMsg: " + ex.Message);
                return false;
            }
        }

        public static uint GetIdMsgFromJson(string jsonMsg)
        {
            try
            {
                string tempStr = jsonMsg.Substring(jsonMsg.IndexOf("IdMsg"));
                string tempStr1 = tempStr.Substring(tempStr.IndexOf(":") + 1);
                string tempStr2 = tempStr1.Substring(0, tempStr1.IndexOf(","));

                if (uint.TryParse(tempStr2, out uint value))
                {
                    Console.Out.WriteLine("GetIdMsgFromJson Number: " + value);
                    return value;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR GetIdMsgFromJson: " + ex.Message);
                return 0;
            }
        }

        public static uint GetIdChlFromJson(string jsonMsg)
        {
            try
            {
                string tempStr = jsonMsg.Substring(jsonMsg.IndexOf("IdChl"));
                string tempStr1 = tempStr.Substring(tempStr.IndexOf(":") + 1);
                string tempStr2 = tempStr1.Substring(0, tempStr1.IndexOf(","));

                if(uint.TryParse(tempStr2, out uint value))
                {
                    return value;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR GetIdChlFromJson: " + ex.Message);
                return 0;
            }
        }

        public static uint GetIdSndFromJson(string jsonMsg)
        {
            try
            {
                string tempStr = jsonMsg.Substring(jsonMsg.IndexOf("IdSnd"));
                string tempStr1 = tempStr.Substring(tempStr.IndexOf(":") + 1);
                string tempStr2 = tempStr1.Substring(0, tempStr1.IndexOf(","));

                if (uint.TryParse(tempStr2, out uint value))
                {
                    return value;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR GetIdSndFromJson: " + ex.Message);
                return 0;
            }
        }
        #endregion

        #region JSON
        public string ToJson()
        {
            try
            {
                string result = JsonSerializer.Serialize(this);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Message) ToJson: " + ex.Message);
                return string.Empty;
            }
        }

        public Message FromJson(string Text)
        {
            string txt = Text.Replace("\0\0", "").Replace("}\0", "}");
            try
            {
                //txt = UtilityAssistant.CleanJSON(txt.Replace("\u002B", "+"));
                Message nwMsg = JsonSerializer.Deserialize<Message>(txt);
                if (nwMsg != null)
                {
                    text = nwMsg.text;
                    length = nwMsg.length;
                    IdChl = nwMsg.IdChl;
                    IdMsg = nwMsg.IdMsg;
                }
                return nwMsg;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Message) FromJson: " + ex.Message + " Text: " + txt);
                return new Message();
            }
        }

        public static Message CreateFromJson(string json)
        {
            try
            {
                string strTemp = json;
                Message msg = new Message();
                return msg.FromJson(strTemp);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Message) CreateFromJson: " + ex.Message);
                return new Message();
            }
        }
        #endregion

        #region Equalizer
        public override bool Equals(object obj)
        {
            // If the passed object is null
            if (obj == null)
            {
                return false;
            }
            if (!(obj is Message))
            {
                return false;
            }
            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            return (this.Text == ((Message)obj).Text)
                && (this.IdMsg == ((Message)obj).IdMsg)
                && (this.IdChl == ((Message)obj).IdChl)
                && (this.Status == ((Message)obj).Status);
        }
        //Overriding the GetHashCode method
        //GetHashCode method generates hashcode for the current object

        public override int GetHashCode()
        {
            //Performing BIT wise OR Operation on the generated hashcode values
            //If the corresponding bits are different, it gives 1.
            //If the corresponding bits are the same, it gives 0.
            return Text.GetHashCode() ^ IdMsg.GetHashCode() ^ IdChl.GetHashCode() ^ Status.GetHashCode() ^ Length.GetHashCode();
        }

        public static bool operator ==(Message onClt1, Message onClt2)
        {
            if (ReferenceEquals(onClt1, null))
            {
                return false;
            }
            if (ReferenceEquals(onClt2, null))
            {
                return false;
            }
            return onClt1.Equals(onClt2);
        }

        public static bool operator !=(Message onClt1, Message onClt2)
        {
            if (ReferenceEquals(onClt1, null))
            {
                return false;
            }
            if (ReferenceEquals(onClt2, null))
            {
                return false;
            }
            return !onClt1.Equals(onClt2);
        }
        #endregion
    }

    //To compare in a distinct of linq
    public class DistinctMessageComparer : IEqualityComparer<Message>
    {

        public bool Equals(Message x, Message y)
        {
            return x.IdMsg == y.IdMsg
                && x.text == y.text
                && x.IdChl == y.IdChl;
        }

        public int GetHashCode(Message obj)
        {
            return obj.IdMsg.GetHashCode()
                ^ obj.text.GetHashCode()
                ^ obj.IdChl.GetHashCode();
        }
    }
}