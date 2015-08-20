/*
 * EasySerializer
 *
 * Author: Anton Holmquist and agladysh
 * Copyright (c) 2013 Anton Holmquist. All rights reserved.
 * http://github.com/antonholmquist/easy-serializer-unity
 * http://github.com/agladysh/easy-serializer-unity
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

public class EasySerializer {

	public static byte[] SerializeObjectToBytes(object serializableObject) {
		EasySerializer.SetEnvironmentVariables();
		
		MemoryStream stream = new MemoryStream();
		
		BinaryFormatter formatter = new BinaryFormatter();
		formatter.Binder = new VersionDeserializationBinder();
		formatter.Serialize(stream, serializableObject);
		
		return stream.GetBuffer ();
		
	}
	
	public static object DeserializeObjectFromBytes(byte[] data) {
		
		EasySerializer.SetEnvironmentVariables();
		
		MemoryStream stream = new MemoryStream (data);
		
		BinaryFormatter formatter = new BinaryFormatter();
		formatter.Binder = new VersionDeserializationBinder();
		return formatter.Deserialize(stream);
	}
	
	
	public static void SerializeObjectToFile(object serializableObject, string filePath) {
		EasySerializer.SetEnvironmentVariables();
		
		Stream stream = File.Open(filePath, FileMode.Create);
		
		BinaryFormatter formatter = new BinaryFormatter();
		formatter.Binder = new VersionDeserializationBinder();
		formatter.Serialize(stream, serializableObject);
		
		stream.Close();
		
	}
	
	public static object DeserializeObjectFromFile(string filePath) {
		
		if (!File.Exists(filePath)) {
			return null;
		}
		
		
		EasySerializer.SetEnvironmentVariables();
		
		Stream stream = null;
		
		try {
			stream = File.Open(filePath, FileMode.Open);
		} 
		
		catch {
			return null;
		}
		
		BinaryFormatter formatter = new BinaryFormatter();
		formatter.Binder = new VersionDeserializationBinder();
		object o = formatter.Deserialize(stream);
		
		stream.Close();
		
		return o;
	}
	
	/* SetEnvironmentVariables required to avoid run-time code generation that will break iOS compatibility
 	 * Suggested by Nico de Poel:
	 * http://answers.unity3d.com/questions/30930/why-did-my-binaryserialzer-stop-working.html?sort=oldest
 	 */
	private static void SetEnvironmentVariables() {
		Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
	}
	
}


/* VersionDeserializationBinder is required to guarantee a fixed serialization assembly name, which Unity likes to randomize on each compile
 * Suggested by TowerOfBricks:
 * http://answers.unity3d.com/questions/8480/how-to-scrip-a-saveload-game-option.html
 * */
public sealed class VersionDeserializationBinder : SerializationBinder 
{ 
	public override Type BindToType( string assemblyName, string typeName )
	{ 
		if ( !string.IsNullOrEmpty( assemblyName ) && !string.IsNullOrEmpty( typeName ) ) 
		{ 
			Type typeToDeserialize = null; 
			
			assemblyName = Assembly.GetExecutingAssembly().FullName; 
			
			// The following line of code returns the type. 
			typeToDeserialize = Type.GetType( String.Format( "{0}, {1}", typeName, assemblyName ) ); 
			
			return typeToDeserialize; 
		} 
		
		return null; 
	} 
}

//USAGE EXAMPLE 1
//using System;
//using System.Runtime.Serialization;
//
//[Serializable ()]
//
//public class ExampleObjectISerializable : ISerializable {
//	
//	public string m_text_1;
//	public string m_text_2;
//	
//	public ExampleObjectISerializable (SerializationInfo info, StreamingContext context) {
//		m_text_1 = info.GetValue("m_text_1", typeof(string)) as string;
//		m_text_2 = info.GetValue("m_text_2", typeof(string)) as string;
//	}
//	
//	public void GetObjectData(SerializationInfo info, StreamingContext context) {
//		info.AddValue("m_text_1", m_text_1);
//		info.AddValue("m_text_2", m_text_2);
//	}
//}


//USAGE EXAMPLE 2
//using System;
//
//[System.SerializableAttribute()]
//
//public class ExampleObjectSerializableAttribute {
//	
//	// Field's that are automatically serialized.
//	public string m_text_1;
//	public string m_text_2;
//	
//	// A field that is not serialized.
//	[NonSerialized()] public string m_text_3; 
//}