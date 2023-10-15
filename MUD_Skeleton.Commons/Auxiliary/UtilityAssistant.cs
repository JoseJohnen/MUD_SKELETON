using System.Numerics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace MUD_Skeleton.Commons.Auxiliary
{
    public class UtilityAssistant
    {
        /// <summary>
        /// Compare Vector3s, usually used in position, to determine what is the directional difference between them, it will return a -1, 0 or 1 depending of the imaginary nature of the answer all compacted in a Vector3
        /// </summary>
        /// <param name="ValueA"></param>
        /// <param name="ValueB"></param>
        /// <returns>Returns a Vector3 with the result of each directional difference (1,0,-1) between ValueA and ValueB on each of his axis independantly</returns>
        public static Vector3 DistanceModifierByVectorComparison(Vector3 ValueA, Vector3 ValueB)
        {
            Vector3 result = Vector3.Zero;
            result.X = DistanceModifierByAxis(ValueA.X, ValueB.X);
            result.Y = DistanceModifierByAxis(ValueA.Y, ValueB.Y);
            result.Z = DistanceModifierByAxis(ValueA.Z, ValueB.Z);
            return result;
        }

        /// <summary>
        /// Compare Vector3s, usually used in position, to determine what is the directional difference between them, it will return a -1, 0 or 1 depending of his distance to 0, all compacted in a Vector3.
        /// </summary>
        /// <param name="ValueA"></param>
        /// <param name="ValueB"></param>
        /// <returns>Returns a Vector3 with the result of each directional cartesian difference (1,0,-1) between ValueA and ValueB on each of his axis independantly</returns>
        public static Vector3 DistanceModifierByCartesianVectorComparison(Vector3 ValueA, Vector3 ValueB)
        {
            Vector3 result = Vector3.Zero;
            result.X = DistanceModifierByCartesianAxis(ValueA.X, ValueB.X);
            result.Y = DistanceModifierByCartesianAxis(ValueA.Y, ValueB.Y);
            result.Z = DistanceModifierByCartesianAxis(ValueA.Z, ValueB.Z);
            return result;
        }

        /// <summary>
        /// Compare floats, usually used in floats of position, to determine what is the directional difference between them, it will return a -1, 0 or 1 depending of the cartesian distance (i.e. which one is closer or farther to 0) 
        /// </summary>
        /// <param name="ValueA">a flota</param>
        /// <param name="ValueB">another float to make the comparison with</param>
        /// <returns>returns 1 if ValueA is farther to 0 or -1 if ValueB is farther to 0, if they are equal it will return 0</returns>
        public static float DistanceModifierByCartesianAxis(float ValueA, float ValueB)
        {
            try
            {
                float evaluator = 0;
                if (ValueA < 0 && ValueB > 0 || ValueA > 0 && ValueB < 0)
                {
                    if (ValueA > ValueB)
                    {
                        evaluator = -1;
                    }
                    else if (ValueA < ValueB)
                    {
                        evaluator = 1;
                    }
                    return evaluator;
                }

                float a, b, c;
                //Determinar si números de mismo signo son positivos o negativos
                if (ValueA > 0)
                {
                    c = 1;
                }
                else if (ValueA < 0)
                {
                    c = -1;
                }
                else
                {
                    if (ValueB > 0)
                    {
                        c = 1;
                    }
                    else if (ValueB < 0)
                    {
                        c = -1;
                    }
                    else
                    {
                        //Si llega acá es porque ambos números son 0
                        return 0;
                    }
                }

                //Si son de igual signo y no son 0 ambos
                a = ValueA < 0 ? ValueA * -1 : ValueA;
                b = ValueB < 0 ? ValueB * -1 : ValueB;

                if (c > 0)
                {
                    if (a > b)
                    {
                        evaluator = 1;
                    }
                    else if (a < b)
                    {
                        evaluator = -1;
                    }
                }
                else if (c < 0)
                {
                    if (a > b)
                    {
                        evaluator = -1;
                    }
                    else if (a < b)
                    {
                        evaluator = 1;
                    }
                }

                return evaluator;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DistanceModifierByCartesianAxis(): " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Compare floats, usually used in floats of position, to determine what is the directional difference between them, it will return a -1, 0 or 1 depending of the imaginary nature of the answer 
        /// </summary>
        /// <param name="ValueA">a flota</param>
        /// <param name="ValueB">another float to make the comparison with</param>
        /// <returns>returns 1 if ValueA is bigger or -1 if ValueB is bigger, if they are equal it will return 0</returns>
        public static float DistanceModifierByAxis(float ValueA, float ValueB)
        {
            try
            {
                float evaluator = 0;
                if (Math.Round(ValueA) > Math.Round(ValueB))
                {
                    evaluator = 1;
                }
                else if (Math.Round(ValueA) < Math.Round(ValueB))
                {
                    evaluator = -1;
                }
                return evaluator;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DistanceModifierByAxis(): " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Compare floats, usually used in Vectors of position, to determine what is the distance between both numerically, it will return a positive
        /// </summary>
        /// <param name="ValueA">a float</param>
        /// <param name="ValueB">another float to make the comparison with</param>
        /// <returns>the distance between the two</returns>
        public static float DistanceComparitorByAxis(float ValueA, float ValueB)
        {
            try
            {
                float evaluator = 0;
                if (ValueA > ValueB)
                {
                    evaluator = ValueA - ValueB;
                }
                else
                {
                    evaluator = ValueB - ValueA;
                }
                return evaluator;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DistanceComparitorByAxis(): " + ex.Message);
                return 0;
            }
        }

        public static Vector2 DistanceComparitorVector2(Vector2 position1, Vector2 position2)
        {
            try
            {
                Vector2 reVect = Vector2.Zero;
                reVect.X = DistanceComparitorByAxis(position1.X, position2.X);
                reVect.Y = DistanceComparitorByAxis(position1.Y, position2.Y);

                return reVect;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) DetectEntityInRange(Vector3, float): " + ex.Message);
                return Vector2.Zero;
            }
        }

        public static Vector3 DistanceComparitorVector3(Vector3 position1, Vector3 position2)
        {
            try
            {
                Vector3 reVect3 = Vector3.Zero;
                reVect3.X = DistanceComparitorByAxis(position1.X, position2.X);
                reVect3.Y = DistanceComparitorByAxis(position1.Y, position2.Y);
                reVect3.Z = DistanceComparitorByAxis(position1.Z, position2.Z);

                return reVect3;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Puppet) DetectEntityInRange(Vector3, float): " + ex.Message);
                return Vector3.Zero;
            }
        }

        #region Quaternion Related
        /// <summary>
        /// Recibe un string en formato 'X:# Y:# Z:# W:#' y lo convierte en un Quaternion con dichos parámetros, luego retorna dicho Quaternion.
        /// </summary>
        /// <param name="information">String containing the quaternion information in format X:# Y:# Z:# W:#</param>
        /// <returns></returns>
        public static Quaternion StringToQuaternion(string information)
        {
            try
            {
                string sQuaternion = "(" + information.Replace(",", ".").Replace(" ", ",").Replace("}", "").Replace("{", "") + ")";
                // Remove the parentheses
                if (sQuaternion.StartsWith("(") && sQuaternion.EndsWith(")"))
                {
                    sQuaternion = sQuaternion.Substring(1, sQuaternion.Length - 2);
                }

                // split the items
                string[] sArray = sQuaternion.Split(',');

                // store as a Vector3
                Quaternion result = new Quaternion(
                    float.Parse(sArray[0].Replace(".", ",").Substring(2)),
                    float.Parse(sArray[1].Replace(".", ",").Substring(2)),
                    float.Parse(sArray[2].Replace(".", ",").Substring(2)),
                    float.Parse(sArray[3].Replace(".", ",").Substring(2)));

                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Quaternion StringToQuaternion(string): " + ex.Message, ConsoleColor.Red);
                return Quaternion.Identity;
            }
        }

        public static Quaternion ToQuaternion(Vector3 v)
        {
            try
            {
                float cy = (float)Math.Cos(v.Z * 0.5);
                float sy = (float)Math.Sin(v.Z * 0.5);
                float cp = (float)Math.Cos(v.Y * 0.5);
                float sp = (float)Math.Sin(v.Y * 0.5);
                float cr = (float)Math.Cos(v.X * 0.5);
                float sr = (float)Math.Sin(v.X * 0.5);

                return new Quaternion
                {
                    W = cr * cp * cy + sr * sp * sy,
                    X = sr * cp * cy - cr * sp * sy,
                    Y = cr * sp * cy + sr * cp * sy,
                    Z = cr * cp * sy - sr * sp * cy
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Quaternion ToQuaternion(Vector3): " + ex.Message, ConsoleColor.Red);
                return Quaternion.Identity;
            }
        }

        public static string QuaternionToXml(Quaternion quaternion, bool isNested = false)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Quaternion));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);
                string result = string.Empty;

                using (StringWriter textWriter = new StringWriter())
                {
                    serializer.Serialize(textWriter, quaternion, ns);
                    result = textWriter.ToString();
                    if (isNested)
                    {
                        result = result.Replace("\"", "\\u0022");
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string QuaternionToXml(Quaternion): " + ex.Message);
                return string.Empty;
            }
        }

        public static Vector3 ToEulerAngles(Quaternion q)
        {
            try
            {
                Vector3 angles = new();

                // roll / x
                double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
                double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
                angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

                // pitch / y
                double sinp = 2 * (q.W * q.Y - q.Z * q.X);
                if (Math.Abs(sinp) >= 1)
                {
                    angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
                }
                else
                {
                    angles.Y = (float)Math.Asin(sinp);
                }

                // yaw / z
                double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
                double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
                angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

                return angles;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Vector3 ToEulerAngles(Quaternion): " + ex.Message);
                return Vector3.Zero;
            }
        }

        public static Quaternion RotateX(float angle)
        {
            //Determina la rotación a partir de un ángulo
            float num = angle * 0.5f;
            return new Quaternion((float)Math.Sin(num), 0f, 0f, (float)Math.Cos(num));
        }

        public static Quaternion RotateY(float angle)
        {
            //Determina la rotación a partir de un ángulo
            float num = angle * 0.5f;
            return new Quaternion(0f, (float)Math.Sin(num), 0f, (float)Math.Cos(num));
        }

        public static Quaternion RotateZ(float angle)
        {
            //Determina la rotación a partir de un ángulo
            float num = angle * 0.5f;
            return new Quaternion(0f, 0f, (float)Math.Sin(num), (float)Math.Cos(num));
        }

        public static Quaternion MultiplyQuaternions(Quaternion left, Quaternion right)
        {
            float lx = left.X;
            float ly = left.Y;
            float lz = left.Z;
            float lw = left.W;
            float rx = right.X;
            float ry = right.Y;
            float rz = right.Z;
            float rw = right.W;

            Quaternion result = new Quaternion();
            result.X = rx * lw + lx * rw + ry * lz - rz * ly;
            result.Y = ry * lw + ly * rw + rz * lx - rx * lz;
            result.Z = rz * lw + lz * rw + rx * ly - ry * lx;
            result.W = rw * lw - (rx * lx + ry * ly + rz * lz);
            return result;
        }
        #endregion

        #region Extraxt Values
        public static string ExtractValues(string instructions, string particle, out string part1, out string part2)
        {
            try
            {
                if (!instructions.Contains(particle))
                {
                    part1 = string.Empty;
                    part2 = string.Empty;
                    return instructions;
                }

                //Extract relevant part
                string particleswithdots = particle + ":";
                string b = instructions.Substring(instructions.IndexOf(particle));
                string d = b.Contains("r/n/") ? b.Substring(0, b.IndexOf("r/n/")) : b;

                //Process relevant part
                string specificRelevantInstruction = d.Substring(particleswithdots.Length);
                part1 = specificRelevantInstruction.Substring(0, 2);
                part2 = specificRelevantInstruction.Substring(2);
                return specificRelevantInstruction;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string ExtractValues(string, string, out string, out string): " + ex.Message);
                part1 = string.Empty;
                part2 = string.Empty;
                return string.Empty;
            }
        }

        public static string ExtractValues(string instructions, string particle, out string part1, out string part2, out string part3)
        {
            try
            {
                if (!instructions.Contains(particle))
                {
                    part1 = string.Empty;
                    part2 = string.Empty;
                    part3 = string.Empty;
                    return instructions;
                }

                //Extract relevant part
                string particleswithdots = particle + ":";
                string b = instructions.Substring(instructions.IndexOf(particle));
                string d = b.Contains("r/n/") ? b.Substring(0, b.IndexOf("r/n/")) : b;

                //Process relevant part
                string specificRelevantInstruction = d.Substring(particleswithdots.Length);
                part1 = specificRelevantInstruction.Substring(0, 2);
                part2 = specificRelevantInstruction.Substring(2, 2);
                part3 = specificRelevantInstruction.Substring(4);
                return specificRelevantInstruction;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string ExtractValues(string, string, out string, out string): " + ex.Message);
                part1 = string.Empty;
                part2 = string.Empty;
                part3 = string.Empty;
                return string.Empty;
            }
        }

        public static string ExtractValues(string instructions, string particle)
        {
            try
            {
                if (!instructions.Contains(particle))
                {
                    return instructions;
                }

                //Extract relevant part
                string particleswithdots = particle + ":";
                string b = instructions.Substring(instructions.IndexOf(particle));
                string d = string.Empty;
                if (b.Contains("\r\n"))
                {
                    d = b.Substring(0, b.IndexOf("\r\n"));
                }
                else
                {
                    d = b;
                }

                //Process relevant part
                string specificRelevantInstruction = d.Substring(particleswithdots.Length);
                return specificRelevantInstruction;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string ExtractValues(string, string): " + ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Extract the value of the specific field in the Json, it eliminates everything else
        /// </summary>
        /// <param name="instruction">the JSON from which its gonna extract the value</param>
        /// <param name="valueName">the name of the field to extract</param>
        /// <returns>the value extacted</returns>
        public static string ExtractValue(string instruction, string valueName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(instruction))
                {
                    return string.Empty;
                }

                string result = instruction;
                if (instruction.Contains("\u0022"))
                {
                    result = CleanJSON(instruction);
                }

                if (result.Contains("\"" + valueName + "\":"))
                {
                    result = result.Substring(result.IndexOf("\"" + valueName + "\":"));
                }
                else if (result.Contains(valueName))
                {
                    result = result.Substring(result.IndexOf(valueName));
                }

                if (result.Contains(","))
                {
                    result = result.Replace(result.Substring(result.IndexOf(",")), "");
                }

                result = UtilityAssistant.PrepareJSON(result);
                result = result.Replace(valueName, "");
                result = result.Replace("\"", "");
                result = result.Replace(":", "");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string ExtractValues(string, string): " + ex.Message);
                return string.Empty;
            }
        }

        public static string ExtractAIInstructionData(string instruction, string valueName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(instruction))
                {
                    return string.Empty;
                }

                string result = instruction;
                result = result.Substring(result.IndexOf("\"" + valueName + "\":"));
                result = result.Replace(result.Substring(result.IndexOf(",")), "");
                string aarg = "\"" + valueName + "\":";
                result = result.Replace(aarg, "");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string ExtractValues(string, string): " + ex.Message);
                return string.Empty;
            }
        }
        #endregion

        // Calculates the shortest difference between two given angles.
        public static float DeltaAngle(float current, float target)
        {
            float delta = Repeat((target - current), 360.0F);
            if (delta > 180.0F)
                delta -= 360.0F;
            return delta;
        }

        // Loops the value t, so that it is never larger than length and never smaller than 0.
        public static float Repeat(float t, float length)
        {
            return Clamp(t - Convert.ToSingle(Math.Floor(t / length)) * length, 0.0f, length);
        }

        // Clamps a value between a minimum float and maximum float value.
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }

        // Clamps value between min and max and returns value.
        // Set the position of the transform to be that of the time
        // but never less than 1 or more than 3
        //
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }

        // Clamps value between 0 and 1 and returns value
        public static float Clamp01(float value)
        {
            if (value < 0F)
                return 0F;
            else if (value > 1F)
                return 1F;
            else
                return value;
        }

        public static float RadiansToDegrees(double radians)
        {
            return Convert.ToSingle(180 / Math.PI * radians);
        }

        public static float DegreesToRadians(double degrees)
        {
            //radians
            return Convert.ToSingle(degrees * (Math.PI / 180));
        }



        /// <summary>
        /// Modulates a quaternion by another.
        /// </summary>
        /// <param name="left">The first quaternion to modulate.</param>
        /// <param name="right">The second quaternion to modulate.</param>
        /// <param name="result">When the moethod completes, contains the modulated quaternion.</param>

        //Cuidado, no funciona con con objetos dentro de objetos TODO: Arreglar eso
        public static string[] CutJson(string jsonToCut)
        {
            try
            {
                string[] result = null;
                if (!string.IsNullOrWhiteSpace(jsonToCut))
                {
                    string tempString = jsonToCut.ReplaceFirst("{", "").ReplaceLast("}", "");

                    if (tempString.Contains(", "))
                    {
                        result = tempString.Split(", ");
                        int i = 0;
                        foreach (string str in result)
                        {
                            result[i] = str.Substring(str.IndexOf(":") + 1).Replace("\"", "");
                            i++;
                        }
                        return result;
                    }

                    if (tempString.Contains(" "))
                    {
                        result = tempString.Split(" ");
                        return result;
                    }
                }

                result = new string[0];
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string[] CutJson(string): " + ex.Message);
                return new string[0];
            }
        }

        //Función original
        /*public static string[] CutJson(string jsonToCut)
        {
            try
            {
                string[] result = null;
                if (!string.IsNullOrWhiteSpace(jsonToCut))
                {
                    string tempString = jsonToCut.Replace("{", "").Replace("}", "");


                    if (tempString.Contains(", "))
                    {
                        result = tempString.Split(", ");
                        int i = 0;
                        foreach (string str in result)
                        {
                            result[i] = str.Substring(str.IndexOf(":") + 1).Replace("\"", "");
                            i++;
                        }
                        return result;
                    }

                    if (tempString.Contains(" "))
                    {
                        result = tempString.Split(" ");
                        return result;
                    }
                }

                result = new string[0];
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string[] CutJson(string): " + ex.Message);
                return new string[0];
            }
        }*/

        public static T XmlToClass<T>(string xml)
        {
            string toProcess = string.Empty;
            try
            {
                toProcess = xml.Replace("xmlns:xsi=http://www.w3.org/2001/XMLSchema-instance xmlns:xsd=http://www.w3.org/2001/XMLSchema", "").Replace("version=1.0", "version=\"1.0\"").Replace("utf-16", "\"utf-16\"").Replace("UTF-8", "\"UTF-8\"");
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                using (StringReader textReader = new StringReader(toProcess))
                {
                    if (textReader != null)
                    {
                        return (T)xmlSerializer.Deserialize(textReader);
                    }
                    else
                    {
                        Console.WriteLine("StringReader is Null: " + xml);
                    }
                }
                return default;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error T XmlToClass<T>: " + ex.Message + " Variable in processing: " + toProcess);
                return default;
            }
        }


        public static string ValidateAndExtractInstructions(string instructions, string signature, out string remainingInstructions)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(instructions))
                {
                    remainingInstructions = instructions;
                    return string.Empty;
                }
                else
                {
                    if (!instructions.Contains(signature))
                    {
                        remainingInstructions = instructions;
                        return string.Empty;
                    }
                }

                string specificRelevantInstruction = ExtractValues(instructions, signature);
                remainingInstructions = instructions.Replace(specificRelevantInstruction, "");
                return specificRelevantInstruction;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ValidateInstructions(string, string, out string): " + ex.Message);
                remainingInstructions = instructions;
                return string.Empty;
            }
        }

        public static string ValidateAndExtractInstructions(string instructions, string signature, out string remainingInstructions, out string part1, out string part2)
        {
            try
            {
                part1 = string.Empty;
                part2 = string.Empty;

                if (string.IsNullOrWhiteSpace(instructions))
                {
                    remainingInstructions = instructions;
                    return string.Empty;
                }
                else
                {
                    if (!instructions.Contains(signature))
                    {
                        remainingInstructions = instructions;
                        return string.Empty;
                    }
                }

                string specificRelevantInstruction = ExtractValues(instructions, signature, out part1, out part2);
                remainingInstructions = instructions.Replace(specificRelevantInstruction, "");
                return specificRelevantInstruction;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ValidateInstructions(string, string, out string, out string, out string): " + ex.Message);
                remainingInstructions = instructions;
                part1 = string.Empty;
                part2 = string.Empty;
                return string.Empty;
            }
        }

        public static string ValidateAndExtractInstructions(string instructions, string signature, out string remainingInstructions, out string part1, out string part2, out string part3)
        {
            try
            {
                part1 = string.Empty;
                part2 = string.Empty;
                part3 = string.Empty;

                if (string.IsNullOrWhiteSpace(instructions))
                {
                    remainingInstructions = instructions;
                    return string.Empty;
                }
                else
                {
                    if (!instructions.Contains(signature))
                    {
                        remainingInstructions = instructions;
                        return string.Empty;
                    }
                }

                string specificRelevantInstruction = ExtractValues(instructions, signature, out part1, out part2, out part3);
                remainingInstructions = instructions.Replace(specificRelevantInstruction, "");
                return specificRelevantInstruction;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ValidateInstructions(string, string, out string, out string, out string, out string): " + ex.Message);
                remainingInstructions = instructions;
                part1 = string.Empty;
                part2 = string.Empty;
                part3 = string.Empty;
                return string.Empty;
            }
        }

        public static string CleanJSON(string json)
        {
            string strTemp = json;
            string strSecTemp = string.Empty;
            string base64text = string.Empty;
            try
            {

                //Verificar si es relevante siquiera correr la función
                if (string.IsNullOrWhiteSpace(strTemp))
                {
                    return string.Empty;
                }
                else if (IsValidJson(strTemp))
                {
                    return json;
                }

                if (strTemp.IndexOf("{") > 1)
                {
                    //Console.WriteLine("Entro a indexOf>1");
                    strSecTemp = strTemp.Substring(0, strTemp.IndexOf("{"));
                    strTemp = strTemp.Replace(strSecTemp, "");
                }

                if (strTemp.Contains("}"))
                {
                    if (strTemp.Length - strTemp.LastIndexOf("}") > 2)
                    {
                        //Console.WriteLine("Entro a LastIndexOf>2");
                        strTemp = strTemp.Replace(strTemp.Substring(strTemp.LastIndexOf("}") + 1), "");
                    }
                }

                if (strTemp.Contains("text"))
                {
                    base64text = strTemp.Substring(strTemp.IndexOf("text"));
                    base64text = base64text.Substring(base64text.IndexOf(":") + 1);
                    if (base64text.Contains("\""))
                    {
                        base64text = base64text.Substring(0, base64text.LastIndexOf("\"")).Replace("\"", "");
                    }
                    if (!string.IsNullOrWhiteSpace(base64text))
                    {
                        if (strTemp.Contains(base64text))
                        {
                            strTemp = strTemp.Replace(base64text, "#$$#|°|#$$#");
                        }
                    }
                }

                /*if (strTemp.Contains("\\"))
                {
                    strTemp = strTemp.Replace("\\", "");
                }*/

                while (strTemp.Contains("\\"))
                {
                    strTemp = strTemp.Replace("\\", "");
                }

                if (strTemp.Contains("u0022"))
                {
                    strTemp = strTemp.Replace("u0022", "\"");
                }

                if (strTemp.Contains("},]"))
                {
                    strTemp = strTemp.Replace("},]", "}]");
                }

                if (strTemp.Contains("u003C"))
                {
                    strTemp = strTemp.Replace("u003C", "<");
                }

                if (strTemp.Contains("u003E"))
                {
                    strTemp = strTemp.Replace("u003E", ">");
                }

                while (strTemp.Contains("\"\""))
                {
                    strTemp = strTemp.Replace("\"\"", "\"");
                }

                if (strTemp.Contains("\"{\""))
                {
                    strTemp = strTemp.Replace("\"{\"", "{\"");
                }

                if (strTemp.Contains("\"}\""))
                {
                    strTemp = strTemp.Replace("\"}\"", "\"}");
                }

                if (strTemp.Contains("},]"))
                {
                    strTemp = strTemp.Replace("},]", "}]");
                }

                /*if (strTemp.Contains("\""))
                {
                    strTemp = strTemp.Replace("\"", "");
                }*/

                //Verificar que algo quedo después de la limpieza, asumiendo que el string no era solo basura
                if (string.IsNullOrWhiteSpace(strTemp))
                {
                    return string.Empty;
                }

                //Cleaning remanents outside of "{ }" so to be certain is valid
                if (strTemp.Contains("\"{\""))
                {
                    strTemp = strTemp.Replace("\"{\"", "{\"");
                }

                if (strTemp.Contains("\"}\""))
                {
                    strTemp = strTemp.Replace("\"}\"", "\"}");
                }

                if (strTemp.Contains("}\""))
                {
                    strTemp = strTemp.Replace("}\"", "}");
                }

                Regex LRegex = new Regex(Regex.Escape("{"));
                int intLRegex = new Regex(Regex.Escape("{")).Matches(strTemp).Count;
                Regex RRegex = new Regex(Regex.Escape("}"));
                int intRRegex = new Regex(Regex.Escape("}")).Matches(strTemp).Count;
                int rslt = intRRegex + intLRegex;
                while (LRegex.Matches(strTemp).Count != RRegex.Matches(strTemp).Count)
                {
                    if (LRegex.Matches(strTemp).Count > RRegex.Matches(strTemp).Count)
                    {
                        //El primero, elimina la primera instancia, el segundo, elimina todo hasta la segunda instancia
                        if (strTemp.Contains("{"))
                        {
                            strTemp = strTemp.Substring(strTemp.IndexOf("{") + 1);
                        }
                        if (strTemp.Contains("{"))
                        {
                            strTemp = strTemp.Substring(strTemp.IndexOf("{"));
                        }
                    }

                    if (LRegex.Matches(strTemp).Count < RRegex.Matches(strTemp).Count)
                    {
                        //El primero, elimina la última instancia, el segundo, elimina todo hasta la anterior
                        //a la última instancia
                        if (strTemp.Contains("}"))
                        {
                            int location = 0;
                            if (strTemp.LastIndexOf("}") - 1 < 2)
                            {
                                location = 1;
                            }
                            else
                            {
                                location = strTemp.LastIndexOf("}") - 1;
                            }
                            strTemp = strTemp.Substring(0, location);
                        }
                        if (strTemp.Contains("}"))
                        {
                            strTemp = strTemp.Substring(0, strTemp.IndexOf("}") + 1);
                        }
                    }

                    if (LRegex.Matches(strTemp).Count == 0 && RRegex.Matches(strTemp).Count == 0)
                    {
                        //Porque en este caso no es JSON en absoluto, pero debería serlo
                        return string.Empty;
                    }
                }

                if (!string.IsNullOrWhiteSpace(base64text))
                {
                    strTemp = strTemp.Replace("#$$#|°|#$$#", base64text);
                }

                return strTemp;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error CleanJSON(string): " + ex.Message);
                return strTemp;
            }
        }

        public static bool IsValidJson(string jsonString)
        {
            try
            {
                JsonDocument.Parse(jsonString);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        public static string Base64Encode(string plainText)
        {
            try
            {
                byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                return Convert.ToBase64String(plainTextBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string Base64Encode(string): String: {0} | Message: {1}", plainText, ex.Message);
                return plainText;
            }
        }

        public static string Base64Decode(string base64EncodedData)
        {
            try
            {
                byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
                string a = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

                bool alreadyDecripted = false;
                if (a.Contains("="))
                {
                    while ((a.Contains("==") || a.LastIndexOf("=") == a.Length - 1) && !alreadyDecripted)
                    {
                        a = Base64Decode(a);
                        alreadyDecripted = true;
                    }
                }

                return a;//System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string Base64Decode(string): String: {0} | Message: {1}", base64EncodedData, ex.Message);
                return base64EncodedData;
            }
        }

        public static bool TryBase64Encode(string plainText, out string result)
        {
            try
            {
                byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                result = Convert.ToBase64String(plainTextBytes);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string TryBase64Encode(string): String: {0} | Message: {1}", plainText, ex.Message);
                result = plainText;
                return false;
            }
        }

        public static bool TryBase64Decode(string base64EncodedData, out string result)
        {
            try
            {
                byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
                result = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

                if (result.Contains("="))
                {
                    while (result.Contains("==") || result.LastIndexOf("=") == result.Length - 1)
                    {
                        result = Base64Decode(result);
                    }
                }

                return true;//System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Error string TryBase64Decode(string): String: {0} | Message: {1}", base64EncodedData, ex.Message);
                Console.ResetColor();
                result = base64EncodedData;
                return false;
            }
        }

        public static Vector3 Vector3Deserializer(string vector3Json)
        {
            string strJson = vector3Json;
            try
            {
                //TODO: Corregir, testear y terminar
                //strJson = reader.GetString();
                strJson = strJson.Replace("\"", "").Replace("{a:", "").Replace("{ a:", "").Replace("}", "").Trim();

                if (strJson.Contains(".�M�"))
                {
                    //Because it's incomplete
                    return Vector3.Zero;
                }

                //strJson = strJson.Replace("´┐¢M´┐¢", "").Replace(".�M�","");
                string[] secondStep = strJson.Replace("<", "").Replace("u003C", "").Replace(">", "").Replace("u003E", "").Replace("\\", "").Replace("\"", "").Split("|");

                string a = secondStep[0].Replace(".", ",").Trim();
                string b = secondStep[1].Replace(".", ",").Trim();
                string c = secondStep[2].Replace(".", ",").Trim();

                //string[] strArray = strJson.Replace("{","").Replace("}","").Split(',');

                Vector3 vector3 = new Vector3((float)Convert.ToDouble(a), (float)Convert.ToDouble(b), (float)Convert.ToDouble(c));
                return vector3;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (Vector3Converter) Read(): {0} Message: {1}", strJson, ex.Message);
                return default;
            }
        }

        public static string PrepareJSON(string json)
        {
            try
            {
                string a = json.Replace("\\u0022", "\"");
                a = a.Replace("\\u003C", "<");
                a = a.Replace("\\u003E", ">");

                a = json.Replace("u0022", "\"");
                a = a.Replace("u003C", "<");
                a = a.Replace("u003E", ">");
                a = a.Replace("\\", "");
                return a;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: static PrepareJSON(string): " + ex.Message);
                return string.Empty;
            }
        }
    }

    public static class StringExtensionMethods
    {
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }

            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static string ReplaceLast(this string Source, string Find, string Replace)
        {
            int place = Source.LastIndexOf(Find);

            if (place == -1)
                return Source;

            return Source.Remove(place, Find.Length).Insert(place, Replace);
        }

        public static bool IsJson(this string source)
        {
            if (source == null)
                return false;

            try
            {
                JsonDocument.Parse(source);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /*public static string ToJson(this Vector3 vector3)
        {
            try
            {
                return "";
            }
            catch(Exception ex) {
                return "";
            }
        }*/
    }

    public static class QuaternionExtensions
    {
        /// <summary>
        /// Creates a quaternion that rotates around the x-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians.</param>
        /// <param name="result">When the method completes, contains the newly created quaternion.</param>
        public static void RotationX(float angle, out Quaternion result)
        {
            float halfAngle = angle * 0.5f;
            result = new Quaternion(MathF.Sin(halfAngle), 0.0f, 0.0f, MathF.Cos(halfAngle));
        }

        /// <summary>
        /// Creates a quaternion that rotates around the x-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians.</param>
        /// <returns>The created rotation quaternion.</returns>
        public static Quaternion RotationX(float angle)
        {
            Quaternion result;
            RotationX(angle, out result);
            return result;
        }

        /// <summary>
        /// Creates a quaternion that rotates around the y-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians.</param>
        /// <param name="result">When the method completes, contains the newly created quaternion.</param>
        public static void RotationY(float angle, out Quaternion result)
        {
            float halfAngle = angle * 0.5f;
            result = new Quaternion(0.0f, MathF.Sin(halfAngle), 0.0f, MathF.Cos(halfAngle));
        }

        /// <summary>
        /// Creates a quaternion that rotates around the y-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians.</param>
        /// <returns>The created rotation quaternion.</returns>
        public static Quaternion RotationY(float angle)
        {
            Quaternion result;
            RotationY(angle, out result);
            return result;
        }

        /// <summary>
        /// Creates a quaternion that rotates around the z-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians.</param>
        /// <param name="result">When the method completes, contains the newly created quaternion.</param>
        public static void RotationZ(float angle, out Quaternion result)
        {
            float halfAngle = angle * 0.5f;
            result = new Quaternion(0.0f, 0.0f, MathF.Sin(halfAngle), MathF.Cos(halfAngle));
        }

        /// <summary>
        /// Creates a quaternion that rotates around the z-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians.</param>
        /// <returns>The created rotation quaternion.</returns>
        public static Quaternion RotationZ(float angle)
        {
            Quaternion result;
            RotationZ(angle, out result);
            return result;
        }

        // Combines rotations /lhs/ and /rhs/.
        public static Quaternion QuaternionXQuaternion (Quaternion lhs, Quaternion rhs)
        {
            return new Quaternion(
                lhs.W * rhs.X + lhs.X * rhs.W + lhs.Y * rhs.Z - lhs.Z * rhs.Y,
                lhs.W * rhs.Y + lhs.Y * rhs.W + lhs.Z * rhs.X - lhs.X * rhs.Z,
                lhs.W * rhs.Z + lhs.Z * rhs.W + lhs.X * rhs.Y - lhs.Y * rhs.X,
                lhs.W * rhs.W - lhs.X * rhs.X - lhs.Y * rhs.Y - lhs.Z * rhs.Z);
        }

        // Rotates the point /point/ with /rotation/.
        public static Vector3 QuaternionXVector3 (Quaternion rotation, Vector3 point)
        {
            float x = rotation.X * 2F;
            float y = rotation.Y * 2F;
            float z = rotation.Z * 2F;
            float xx = rotation.X * x;
            float yy = rotation.Y * y;
            float zz = rotation.Z * z;
            float xy = rotation.X * y;
            float xz = rotation.X * z;
            float yz = rotation.Y * z;
            float wx = rotation.W * x;
            float wy = rotation.W * y;
            float wz = rotation.W * z;

            Vector3 res;
            res.X = (1F - (yy + zz)) * point.X + (xy - wz) * point.Y + (xz + wy) * point.Z;
            res.Y = (xy + wz) * point.X + (1F - (xx + zz)) * point.Y + (yz - wx) * point.Z;
            res.Z = (xz - wy) * point.X + (yz + wx) * point.Y + (1F - (xx + yy)) * point.Z;
            return res;
        }


        public static bool IsClockwiseOrCounterClockwiseY(this Quaternion B, Quaternion A)
        {
            //Quaternion A; //first Quaternion - this is your desired rotation
            //Quaternion B; //second Quaternion - this is your current rotation

            // define an axis, usually just up
            Vector3 axis = new Vector3(0.0f, 1.0f, 0.0f);

            // mock rotate the axis with each 
            Vector3 vecA = QuaternionExtensions.QuaternionXVector3(A, axis);
            Vector3 vecB = QuaternionExtensions.QuaternionXVector3(B, axis);

            // now we need to compute the actual 2D rotation projections on the base plane
            float angleA = UtilityAssistant.RadiansToDegrees(MathF.Atan2(vecA.X, vecA.Z));
            float angleB = UtilityAssistant.RadiansToDegrees(MathF.Atan2(vecB.X, vecB.Z));

            // get the signed difference in these angles
            var angleDiff = UtilityAssistant.DeltaAngle(angleA, angleB);

            if (angleDiff > 0)
            {
                //is clockwise
                return true;
            }
            //is counter clockwise
            return false;
        }

        public static Matrix2x2 QuaternionToMatrix2x2(this Quaternion q)
        {
            // Extract the components of the quaternion
            float w = q.W;
            float x = q.X;
            float y = q.Y;
            float z = q.Z;

            // Calculate the elements of the 2x2 rotation matrix
            float m11 = 1 - 2 * (y * y + z * z);
            float m12 = 2 * (x * y - w * z);
            float m21 = 2 * (x * y + w * z);
            float m22 = 1 - 2 * (x * x + z * z);

            // Create and return the 2x2 rotation matrix
            return new Matrix2x2(m11, m12, m21, m22);
        }

        public static Matrix3x3 QuaternionToMatrix3x3(Quaternion q)
        {
            // Extract the components of the quaternion
            float w = q.W;
            float x = q.X;
            float y = q.Y;
            float z = q.Z;

            // Calculate the elements of the 3x3 rotation matrix
            float m11 = 1 - 2 * (y * y + z * z);
            float m12 = 2 * (x * y - w * z);
            float m13 = 2 * (x * z + w * y);

            float m21 = 2 * (x * y + w * z);
            float m22 = 1 - 2 * (x * x + z * z);
            float m23 = 2 * (y * z - w * x);

            float m31 = 2 * (x * z - w * y);
            float m32 = 2 * (y * z + w * x);
            float m33 = 1 - 2 * (x * x + y * y);

            // Create and return the 3x3 rotation matrix
            return new Matrix3x3(m11, m12, m13, m21, m22, m23, m31, m32, m33);
        }
    }

}