using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace AyoController.Classes
{
	public class IniFile
	{

#region "Declarations"

		// *** Lock for thread-safe access to file and local cache ***
		private readonly object _mLock = new object();

		// *** File name ***
		private string _mFileName = null;
		public string FileName
		{
			get
			{
				return _mFileName;
			}
		}

		// *** Lazy loading flag ***
		private bool _mLazy = false;

		// *** Local cache ***
		private readonly Dictionary<string, Dictionary<string, string>> _mSections = new Dictionary<string,Dictionary<string, string>>(); 

		// *** Local cache modified flag ***
		private bool _mCacheModified = false;

#endregion

#region "Methods"

		// *** Constructor ***
		public IniFile(string fileName)
		{
			Initialize(fileName, false);
		}

		public IniFile(string fileName, bool lazy)
		{
			Initialize(fileName, lazy);
		}

		// *** Initialization ***
		private void Initialize (string fileName, bool lazy)
		{
			_mFileName = fileName;
			_mLazy = lazy;
			if (!_mLazy) Refresh();
		}

		// *** Read file contents into local cache ***
		public void Refresh()
		{
			lock (_mLock)
			{
				StreamReader sr = null;
				try
				{
					// *** Clear local cache ***
					_mSections.Clear();

					// *** Open the INI file ***
					try
					{
						sr = new StreamReader(_mFileName);
					}
					catch (FileNotFoundException)
					{
						return;
					}

					// *** Read up the file content ***
					Dictionary<string, string> currentSection = null;
					string s;
					while ((s = sr.ReadLine()) != null)
					{
						s = s.Trim();
						
						// *** Check for section names ***
						if (s.StartsWith("[") && s.EndsWith("]"))
						{
							if (s.Length > 2)
							{
								string sectionName = s.Substring(1,s.Length-2);
								
								// *** Only first occurrence of a section is loaded ***
								if (_mSections.ContainsKey(sectionName))
								{
									currentSection = null;
								}
								else
								{
									currentSection = new Dictionary<string, string>();
									_mSections.Add(sectionName,currentSection);
								}
							}
						}
						else if (currentSection != null)
						{
							// *** Check for key+value pair ***
							int i;
							if ((i=s.IndexOf('=')) > 0)
							{
								int j = s.Length - i - 1;
								string key = s.Substring(0,i).Trim();
								if (key.Length  > 0)
								{
									// *** Only first occurrence of a key is loaded ***
									if (!currentSection.ContainsKey(key))
									{
										string value = (j > 0) ? (s.Substring(i+1,j).Trim()) : ("");
										currentSection.Add(key,value);
									}
								}
							}
						}
					}
				}
				finally
				{
					// *** Cleanup: close file ***
					if (sr != null) sr.Close();
					sr = null;
				}
			}
		}
		
		// *** Flush local cache content ***
		public void Flush()
		{
			lock(_mLock)
			{
				// *** If local cache was not modified, exit ***
				if (!_mCacheModified) return;				
				_mCacheModified=false;

				// *** Open the file ***
				StreamWriter sw = new StreamWriter(_mFileName);

				try
				{
					// *** Cycle on all sections ***
					bool first = false;
					foreach (KeyValuePair<string, Dictionary<string, string>> sectionPair in _mSections)
					{
						Dictionary<string, string> section = sectionPair.Value;
						if (first) sw.WriteLine();
						first = true;

						// *** Write the section name ***
						sw.Write('[');
						sw.Write(sectionPair.Key);
						sw.WriteLine(']');
					
						// *** Cycle on all key+value pairs in the section ***
						foreach (KeyValuePair<string, string> valuePair in section)
						{
							// *** Write the key+value pair ***
							sw.Write(valuePair.Key);
							sw.Write('=');
							sw.WriteLine(valuePair.Value);
						}
					}
			    }
				finally
				{
					// *** Cleanup: close file ***
					if (sw != null) sw.Close();
					sw = null;
				}
			}
		}
		
		// *** Read a value from local cache ***
		public string GetValue(string sectionName, string key, string defaultValue)
		{
			// *** Lazy loading ***
			if (_mLazy)
			{
				_mLazy = false;
				Refresh();
			}

			lock (_mLock)
			{
				// *** Check if the section exists ***
				Dictionary<string, string> section;
				if (!_mSections.TryGetValue(sectionName, out section)) return defaultValue;

				// *** Check if the key exists ***
				string value;
				if (!section.TryGetValue(key, out value)) return defaultValue;
			
				// *** Return the found value ***
				return value;
			}
		}

		// *** Insert or modify a value in local cache ***
		public void SetValue(string sectionName, string key, string value)
		{
			// *** Lazy loading ***
			if (_mLazy)
			{
				_mLazy = false;
				Refresh();
			}

			lock (_mLock)
			{
				// *** Flag local cache modification ***
				_mCacheModified = true;

				// *** Check if the section exists ***
				Dictionary<string, string> section;
				if (!_mSections.TryGetValue(sectionName, out section))
				{
					// *** If it doesn't, add it ***
					section = new Dictionary<string, string>();
					_mSections.Add(sectionName,section);
				}

				// *** Modify the value ***
				if (section.ContainsKey(key)) section.Remove(key);
				section.Add(key, value);
			}
		}

		// *** Encode byte array ***
		private string EncodeByteArray(byte[] value)
		{
			if (value == null) return null;

			StringBuilder sb = new StringBuilder();
			foreach (byte b in value)
			{
				string hex = Convert.ToString(b,16);
				int l = hex.Length;
				if (l > 2)
				{
					sb.Append(hex.Substring(l-2,2));
				}
				else
				{
				    if (l < 2) sb.Append("0");
					sb.Append(hex);
				}
			}
			return sb.ToString();
		}

		// *** Decode byte array ***
		private byte[] DecodeByteArray(string value)
		{
			if (value == null) return null;

			int l = value.Length;
			if (l < 2) return new byte[] { };
			
			l /= 2;
			byte[] result = new byte[l];
			for (int i=0; i<l; i++) result[i] = Convert.ToByte(value.Substring(i*2,2),16);
			return result;
		}

		// *** Getters for various types ***
		public bool GetValue(string sectionName, string key, bool defaultValue)
		{
			string stringValue=GetValue(sectionName, key, defaultValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
			int value;
			if (int.TryParse(stringValue, out value)) return (value != 0);
			return defaultValue;
		}

		public int GetValue(string sectionName, string key, int defaultValue)
		{
			string stringValue=GetValue(sectionName, key, defaultValue.ToString(CultureInfo.InvariantCulture));
			int value;
			if (int.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) return value;
			return defaultValue;
		}

		public double GetValue(string sectionName, string key, double defaultValue)
		{
			string stringValue=GetValue(sectionName, key, defaultValue.ToString(CultureInfo.InvariantCulture));
			double value;
			if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) return value;
			return defaultValue;
		}

		public byte[] GetValue(string sectionName, string key, byte[] defaultValue)
		{
			string stringValue = GetValue(sectionName, key, EncodeByteArray(defaultValue));
			try
			{
				return DecodeByteArray(stringValue);
			}
			catch (FormatException)
			{
				return defaultValue;
			}
		}

		// *** Setters for various types ***
		public void SetValue(string sectionName, string key, bool value)
		{
			SetValue(sectionName, key, (value) ? ("1") : ("0"));
		}

		public void SetValue(string sectionName, string key, int value)
		{
			SetValue(sectionName, key, value.ToString(CultureInfo.InvariantCulture));
		}

		public void SetValue(string sectionName, string key, double value)
		{
			SetValue(sectionName, key, value.ToString(CultureInfo.InvariantCulture));
		}

		public void SetValue(string sectionName, string key, byte[] value)
		{
			SetValue(sectionName, key, EncodeByteArray(value));
		}

#endregion

	}
}

