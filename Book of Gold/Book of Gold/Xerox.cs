﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;

namespace Book_of_Gold
{
    public sealed class Xerox
    {
        Xerox() { }

        public static Xerox Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        class Nested
        {
            static Nested() { }

            internal static readonly Xerox instance = new Xerox();
        }

        public object Photocopy(object original)
        {
            MemoryStream ms = new MemoryStream();
            object retobj;
            using (BsonWriter w = new BsonWriter(ms))
            {
                JsonSerializer s = new JsonSerializer();
                s.Serialize(w, original);
            }
            string sData = Convert.ToBase64String(ms.ToArray());
            byte[] bData = Convert.FromBase64String(sData);
            ms = new MemoryStream(bData);
            using (BsonReader r = new BsonReader(ms))
            {
                JsonSerializer s = new JsonSerializer();
                retobj = s.Deserialize(r);
            }
            ms.Close();
            return retobj;
        }
    }
}