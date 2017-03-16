using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ImageSharingWebRole.Models
{
    [Serializable()]
    public class ValidationInfo 
    {
        public string ImageCaption { get; set; }
        public bool isValidated { get; set; }
        
        /// <summary>
        /// Convert an object to Byte[]
        /// </summary>
        /// <returns></returns>
        public byte[] ClassToByteArray()
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                System.Xml.Serialization.XmlSerializer xmlS = new System.Xml.Serialization.XmlSerializer(typeof(ValidationInfo));
                System.Xml.XmlTextWriter xmlTW = new System.Xml.XmlTextWriter(ms, Encoding.UTF8);

                xmlS.Serialize(xmlTW, this);
                ms = (MemoryStream)xmlTW.BaseStream;

                return ms.ToArray();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
