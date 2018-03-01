using System;
using System.Collections;
using System.Collections.Generic;

namespace LLE 
{
    namespace Util
	{
        public class DebugMenuException : Exception
        {
            public DebugMenuException(string message)
                :
                base(message)
            {
            }
        }

        public class DebugMenuQuitException : Exception
        {
        }

        public class DebugMenu
		{
            public delegate void MenuItemDelegate();
            private List<MenuItem> m_menuItemList = new List<MenuItem>();
			private string m_title;

            private class MenuItem
            {
                public MenuItemDelegate m_menuDelegate;
                public string m_text;
                public bool m_enabled;

                public MenuItem(string aText, MenuItemDelegate aDelegate, bool aEnabled)
                {
                    m_menuDelegate = aDelegate;
                    m_text = aText;
                    m_enabled = aEnabled;
                }
            };

            public DebugMenu(string aTitle)
			{
				m_title = aTitle;
			}

			public bool Run()
			{
                bool status = true;

				DisplayMenu();
				while (true) 
				{
					Prompt();
					string input = Console.ReadLine();
                    if (input == null)
                    {
                        status = false;
                        break;
                    }
					else if (input.Length == 0) 
					{
						// ignore
					}
					else if (input == "q") 
					{
						break;
					}
                    else if (input == "l")
                    {
                        DisplayLogMenu();
                    }
                    else if (input == "?")
                    {
                        DisplayMenu();
                    }
                    else 
					{
						// convert the input into a menu item number
						try 
						{
							int item = Int32.Parse(input);

							// check if the item is invalid
							if (item < 1 || item > m_menuItemList.Count) 
							{
								throw new DebugMenuException("Selection out of range");
							}

							// execute the item
                            ((MenuItem)m_menuItemList[item - 1]).m_menuDelegate();
						}
                        catch (DebugMenuQuitException)
                        {
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
					}
				}

				// clean up
				OnQuit();

                return status;
			}

            public void Stop()
            {
            }

			protected virtual void OnQuit()
			{
			}

			protected void PushMenu(DebugMenu aMenu)
			{
				aMenu.Run();
				DisplayMenu();
			}

            public int AddMenuItem(string aText, MenuItemDelegate aDelegate, bool aEnabled)
            {
                m_menuItemList.Add(new MenuItem(aText, aDelegate, aEnabled));
                return m_menuItemList.Count - 1;
            }

            public int AddMenuItem(string aText, MenuItemDelegate aDelegate)
            {
                return AddMenuItem(aText, aDelegate, true);
            }

            public void EnableMenuItem(int aItem, bool aEnabled)
            {
                m_menuItemList[aItem].m_enabled = aEnabled;
            }

			public byte PromptByte()
			{
				return PromptByte("");
			}

            public byte PromptByte(
				string aPrompt
				)
			{
                return PromptByte(aPrompt, 0);
			}

            public byte PromptByte(
				string aPrompt,
				byte aDefault
				)
			{
                return PromptByte(aPrompt, aDefault, byte.MinValue);
			}

            public byte PromptByte(
				string aPrompt,
				byte aDefault,
				byte aMin
				)
			{
                return PromptByte(aPrompt, aDefault, aMin, byte.MinValue);
			}

            public byte PromptByte(
                string aPrompt,
                byte aDefault,
                byte aMin,
                byte aMax
                )
            {
                return (byte)PromptLong(aPrompt, aDefault, aMin, aMax);
            }

            public short PromptShort()
			{
                return PromptShort("");
			}

            public short PromptShort(
				string aPrompt
				)
			{
                return PromptShort(aPrompt, 0);
			}

            public short PromptShort(
				string aPrompt,
				short aDefault
				)
			{
                return PromptShort(aPrompt, aDefault, short.MinValue);
			}

            public short PromptShort(
				string aPrompt,
				short aDefault,
				short aMin
				)
			{
                return PromptShort(aPrompt, aDefault, aMin, short.MaxValue);
			}

            public short PromptShort(
				string aPrompt,
				short aDefault,
				short aMin,
				short aMax
				)
			{
                return (short)PromptLong(aPrompt, aDefault, aMin, aMax);
			}

            public int PromptInt()
			{
				return PromptInt("");
			}

            public int PromptInt(
				string aPrompt
				)
			{
                return PromptInt(aPrompt, 0);
			}

            public int PromptInt(
				string aPrompt,
				int aDefault
				)
			{
                return PromptInt(aPrompt, aDefault, int.MinValue);
			}

            public int PromptInt(
				string aPrompt,
				int aDefault,
				int aMin
				)
			{
                return PromptInt(aPrompt, aDefault, aMin, int.MaxValue);
			}

            public int PromptInt(
				string aPrompt,
				int aDefault,
				int aMin,
				int aMax
				)
			{
                return (int)PromptLong(aPrompt, aDefault, aMin, aMax);
			}

            public long PromptLong()
			{
                return PromptLong("");
			}

            public long PromptLong(
				string aPrompt
				)
			{
                return PromptLong(aPrompt, 0);
			}

            public long PromptLong(
				string aPrompt,
				long aDefault
				)
			{
                return PromptLong(aPrompt, aDefault, long.MinValue);
			}

            public long PromptLong(
				string aPrompt,
				long aDefault,
				long aMin
				)
			{
                return PromptLong(aPrompt, aDefault, aMin, long.MaxValue);
			}

            public long PromptLong(
				string aPrompt,
				long aDefault,
				long aMin,
				long aMax
				)
			{
				// don't bother prompting for degenerate cases
				if (aMin == aMax) 
				{
					return aMin;
				}

				// if prompt is empty, construct our own
				string prompt = aPrompt;
				if (prompt == null || prompt.Length == 0) 
				{
					prompt = "Enter an integer";
				}
				prompt += " [Default=" + aDefault + "]: ";

				while (true) 
				{
					Console.Write(prompt);
					string input = Console.ReadLine();

					// check for default
					if (input.Length == 0) 
					{
						return aDefault;
					}
						// check for an early exit
					else if (input == "q") 
					{
						throw new DebugMenuException("Operation cancelled by user");
					}
						// try to convert to an int
					else 
					{
						try 
						{
							long tmp = Int64.Parse(input);
							if (tmp < aMin || tmp > aMax) 
							{
								throw new DebugMenuException("Value out of range");
							}
							return tmp;
						}
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
				}
			}

            public float PromptFloat()
			{
                return PromptFloat("");
			}

            public float PromptFloat(
				string aPrompt
				)
			{
                return PromptFloat(aPrompt, 0.0F);
			}

            public float PromptFloat(
				string aPrompt,
				float aDefault
				)
			{
                return PromptFloat(aPrompt, aDefault, float.MinValue);
			}

            public float PromptFloat(
				string aPrompt,
				float aDefault,
				float aMin
				)
			{
                return PromptFloat(aPrompt, aDefault, aMin, float.MaxValue);
			}

            public float PromptFloat(
				string aPrompt,
				float aDefault,
				float aMin,
				float aMax
				)
			{
                return (float)PromptDouble(aPrompt, aDefault, aMin, aMax);
			}

            public double PromptDouble()
			{
				return PromptDouble("");
			}

            public double PromptDouble(
				string aPrompt
				)
			{
                return PromptDouble(aPrompt, 0.0);
			}

            public double PromptDouble(
				string aPrompt,
				double aDefault
				)
			{
                return PromptDouble(aPrompt, aDefault, double.MinValue);
			}

            public double PromptDouble(
				string aPrompt,
				double aDefault,
				double aMin
				)
			{
                return PromptDouble(aPrompt, aDefault, aMin, double.MaxValue);
			}

            public double PromptDouble(
				string aPrompt,
				double aDefault,
				double aMin,
				double aMax
				)
			{
				// don't bother prompting for degenerate cases
				if (aMin == aMax) 
				{
					return aMin;
				}

				// if prompt is empty, construct our own
				string prompt = aPrompt;
				if (prompt == null || prompt.Length == 0) 
				{
					prompt = "Enter a real number";
				}
				prompt += " [Default=" + aDefault + "]: ";

				while (true) 
				{
					Console.Write(prompt);
					string input = Console.ReadLine();

					// check for default
					if (input.Length == 0) 
					{
						return aDefault;
					}
						// check for an early exit
					else if (input == "q") 
					{
						throw new DebugMenuException("Operation cancelled by user");
					}
						// try to convert to an int
					else 
					{
						try 
						{
							double tmp = Double.Parse(input);
							if (tmp < aMin || tmp > aMax) 
							{
								throw new DebugMenuException("Value out of range");
							}
							return tmp;
						}
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
				}
			}

            public bool PromptBool()
			{
				return PromptBool("");
			}

            public bool PromptBool(
				string aPrompt
				)
			{
                return PromptBool(aPrompt, false);
			}

            public bool PromptBool(
				string aPrompt,
				bool aDefault
				)
			{
				// if prompt is empty, construct our own
				string prompt = aPrompt;
				if (prompt == null || prompt.Length == 0) 
				{
					prompt = "Enter a boolean";
				}
				prompt += " [Default=";
				prompt += BoolToString(aDefault);
				prompt += "]: ";

				while (true) 
				{
					Console.Write(prompt);
					string input = Console.ReadLine();

					// check for default
					if (input.Length == 0) 
					{
						return aDefault;
					}
						// check for an early exit
					else if (input == "q") 
					{
						throw new DebugMenuException("Operation cancelled by user");
					}
					else if (input == BoolToString(true)) 
					{
						return true;
					}
					else if (input == BoolToString(false)) 
					{
						return false;
					}
					else 
					{
						Console.WriteLine("Invalid entry");
					}
				}
			}

            public string PromptString()
			{
                return PromptString("");
			}

            public string PromptString(
				string aPrompt
				)
			{
				return PromptString(aPrompt, "");
			}

            public string PromptString(
				string aPrompt,
				string aDefault
				)
			{
                return PromptString(aPrompt, aDefault, true);
			}

            public string PromptString(
				string aPrompt,
				string aDefault,
				bool aEnableDefault
				)
			{
                return PromptString(aPrompt, aDefault, aEnableDefault, 0);
			}

            public string PromptString(
				string aPrompt,
				string aDefault,
				bool aEnableDefault,
				int aMinLength
				)
			{
                return PromptString(aPrompt, aDefault, aEnableDefault, 0, 32767);
			}

            public string PromptString(
				string aPrompt,
				string aDefault,
				bool aEnableDefault,
				int aMinLength,
				int aMaxLength
				)
			{
                return PromptString(aPrompt, aDefault, aEnableDefault, 0, 32767, "q");
			}

            public string PromptString(
				string aPrompt,
				string aDefault,
				bool aEnableDefault,
				int aMinLength,
				int aMaxLength,
				string aQuitString
				)
			{
				// if prompt is empty, construct our own
				string prompt = aPrompt;
				if (prompt == null || prompt.Length == 0) 
				{
					prompt = "Enter a string";
				}
				if (aEnableDefault) 
				{
					prompt += " [Default=";
					prompt += aDefault;
					prompt += "]: ";
				}
				else 
				{
					prompt += " : ";
				}

				while (true) 
				{
					Console.Write(prompt);
					string input = Console.ReadLine();

					// check for default
					if (aEnableDefault && input.Length == 0) 
					{
						return aDefault;
					}
						// check for an early exit
					else if (aQuitString != null && aQuitString.Length > 0 && input == aQuitString) 
					{
						throw new DebugMenuException("Operation cancelled by user");
					}
						// else check the length
					else if (input.Length >= aMinLength && input.Length <= aMaxLength) 
					{
						return input;
					}
					else 
					{
						Console.WriteLine("Invalid entry");
					}
				}
			}

            public Object PromptList(
				IList aList
				)
			{
                return PromptList(aList, "");
			}

            public Object PromptList(
                IList aList,
				string aPrompt
				)
			{
                return PromptList(aList, aPrompt, null);
			}

            public Object PromptList(
                IList aList,
				string aPrompt,
				Object aDefault
				)
			{
				// if prompt is empty, construct our own
				string prompt = aPrompt;
				if (prompt == null || prompt.Length == 0) 
				{
					prompt = "Select an item";
				}
				if (aDefault != null) 
				{
					int i;
					for (i=0; i<aList.Count; i++) 
					{
						if (aList[i] == aDefault) 
						{
							break;
						}
					}
					if (i < aList.Count) 
					{
						prompt += " [Default=";
						prompt += aDefault;
						prompt += "]: ";
					}
					else 
					{
						prompt += " : ";
					}
				}
				else 
				{
					prompt += " : ";
				}

				while (true) 
				{
					// display the list
					int i;
					for (i=0; i<aList.Count; i++) 
					{
						Console.WriteLine((i+1) + ") " + aList[i]);
					}
					Console.Write(prompt);
					string input = Console.ReadLine();

					// check for default
					if (aDefault != null && input.Length == 0) 
					{
						return aDefault;
					}
						// check for an early exit
					else if (input == "q") 
					{
						throw new DebugMenuException("Operation cancelled by user");
					}
						// try to convert to an int
					else 
					{
						try
						{
							int tmp = Int32.Parse(input);

							if (tmp < 1 || tmp > aList.Count)
							{
								throw new DebugMenuException("Value out of range");
							}

							return aList[tmp-1];
						}
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
				}
			}

            public int PromptListIndex(
                IList aList
                )
            {
                return PromptListIndex(aList, "");
            }

            public int PromptListIndex(
                IList aList,
                string aPrompt
                )
            {
                return PromptListIndex(aList, aPrompt, 0);
            }

            public int PromptListIndex(
                IList aList,
                string aPrompt,
                int aDefault
                )
            {
                // if prompt is empty, construct our own
                string prompt = aPrompt;
                if (prompt == null || prompt.Length == 0)
                {
                    prompt = "Select an item";
                }
                prompt += " [Default=";
                prompt += aList[aDefault];
                prompt += "]: ";

                while (true)
                {
                    // display the list
                    int i;
                    for (i = 0; i < aList.Count; i++)
                    {
                        Console.WriteLine((i + 1) + ") " + aList[i]);
                    }
                    Console.Write(prompt);
                    string input = Console.ReadLine();

                    // check for default
                    if (input.Length == 0)
                    {
                        return aDefault;
                    }
                    // check for an early exit
                    else if (input == "q")
                    {
                        throw new DebugMenuException("Operation cancelled by user");
                    }
                    // try to convert to an int
                    else
                    {
                        try
                        {
                            int tmp = Int32.Parse(input);

                            if (tmp < 1 || tmp > aList.Count)
                            {
                                throw new DebugMenuException("Value out of range");
                            }

                            return tmp - 1;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
            }

            protected virtual void DisplayTitle()
			{
				Console.WriteLine("");
				Console.WriteLine(m_title);
				Console.WriteLine("");
			}

			protected virtual void DisplayMenu()
			{
				DisplayTitle();

				IEnumerator itr = m_menuItemList.GetEnumerator();
				int i=1;
				while (itr.MoveNext()) {
					MenuItem menuItem = (MenuItem)itr.Current;
					if (menuItem.m_enabled) {
						Console.WriteLine((i++) + ") " + menuItem.m_text);
					}
				}
                Console.WriteLine("q) quit");
                Console.WriteLine("l) change log level");
                Console.WriteLine("?) help");
			}

            void DisplayLogMenu()
            {
                string[] logLevels = new String[] {
                    "Debug",
                    "Info",
                    "Warning",
                    "Error"
                };

	            try
                {
		            int logLevel = PromptListIndex(logLevels, "Select the new log level");
		            Log.Instance.SetLogLevel((LogLevel)logLevel);
	            }
	            catch (DebugMenuException)
	            {
	            }
            }

			protected virtual void Prompt()
			{
				Console.Write("> ");
			}

			protected string BoolToString(bool aBool)
			{
				return aBool ? "true" : "false";
			}
		}
	}
}
