using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Reflection;

namespace ReflectionUtility
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        ObservableCollection<string> dataList;
        string[] firstList;
        Assembly asm = null;

        /// <summary>Инициализирует новый экземпляр класса <see cref="MainWindow"/>.</summary>
        public MainWindow()
        {
            InitializeComponent();
            string[] dlls = {"mscorlib.dll", "System.dll", "System.Core.dll", "System.Xml.dll", "System.Xaml.dll", "WindowsBase.dll",
            "PresentationCore.dll", "PresentationFramework.dll" , "Microsoft.CSharp.dll", "System.Data.dll", "System.Net.Http.dll", "System.Data.DataSetExtensions.dll",
            "System.Xml.Linq.dll"};
            Dlls.ItemsSource = dlls;
            Dlls.SelectedItem = dlls[0];
        }

        /// <summary>Позволяет выбирать сборки для анализа типов.</summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Экземпляр <see cref="SelectionChangedEventArgs"/>, содержащий данные события.</param>
        private void Dlls_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            asm = Assembly.LoadFrom(comboBox.SelectedItem.ToString());

            dataList = new ObservableCollection<string>();
            foreach (var type in asm.GetTypes())
            {
                dataList.Add(type.FullName);
            }
            list.ItemsSource = dataList;
            firstList = new string[dataList.Count];
            dataList.CopyTo(firstList, 0);
            Type_TextChanged(null, null);
        }

        /// <summary>  Фильтрует типы сборки в соответствии с вводимым текстом.</summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Экземпляр <see cref="TextChangedEventArgs"/>, содержащий данные события.</param>
        private async void Type_TextChanged(object sender, TextChangedEventArgs e)
        {
            string pattern = Type.Text;
            List<string> newlist = new List<string>();
            await Task.Run(() =>
            {
                foreach (string type in firstList)
                {
                    if (type.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                        newlist.Add(type);
                }
            });
            var list = dataList.Except(newlist).ToList();
            foreach (string type in list)
            {
                dataList.Remove(type);
            }
            list = newlist.Except(dataList).ToList();
            foreach (string type in list)
            {
                dataList.Add(type);
            }
        }

        /// <summary>Анализирует заданный тип и выводит информацию о нём.</summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Экземпляр <see cref="SelectionChangedEventArgs"/>, содержащий данные события.</param>
        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(list.SelectedItem != null)
            {
                List<string> MethodsInPropNames = null;
                
                StringBuilder data = new StringBuilder(100);
                Type researchType = asm.GetType(list.SelectedItem as string, true, true);
                if (researchType.GetProperties().Length > 0)
                    MethodsInPropNames = new List<string>();
                if(researchType.CustomAttributes.Count() > 0)
                {
                    AddAttributes(researchType,data);
                }
                if (researchType.IsPublic)
                    data.Append("public ");
                if (researchType.IsAbstract && !researchType.IsInterface && !researchType.IsSealed)
                    data.Append("abstract ");
                if (researchType.IsSealed && !researchType.IsAbstract)
                    data.Append("sealed ");
                if (researchType.IsAbstract && researchType.IsSealed)
                    data.Append("static ");
                if (researchType.IsValueType && !researchType.IsEnum)
                    data.Append("struct ");
                if (researchType.IsClass)
                    data.Append("class ");
                if (researchType.IsInterface)
                    data.Append("interface ");
                if (researchType.IsEnum)
                    data.Append("enum ");
                if (researchType.IsPointer)
                    data.Append("*");
                data.Append(list.SelectedItem as string);

                if(researchType.IsGenericType)
                    data.Append(AddGenericTypes(researchType));
                if(researchType.BaseType != null)
                {
                    data.Append(" : ");
                    data.Append(researchType.BaseType.Name);
                    if (researchType.GetInterfaces().Length > 0)
                    {
                        data.Append(", ");
                        int count = 0;
                        foreach (var interf in researchType.GetInterfaces())
                        {
                            data.Append(interf.Name);
                            if (interf.IsGenericType)
                                data.Append(AddGenericTypes(interf));
                            if (++count < researchType.GetInterfaces().Length)
                            {
                                data.Append(", ");
                            }
                        }
                    }
                }

                data.Append(Environment.NewLine + "{" + Environment.NewLine + "\tПоля " + Environment.NewLine);
                foreach (FieldInfo field in researchType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    if (field.GetCustomAttributes().Count() > 0)
                        AddAttributes(field, data);
                    data.Append("\t\t");
                    if (field.IsPublic)
                        data.Append("public ");
                    if (field.IsPrivate)
                        data.Append("private ");
                    if (field.IsFamily)
                        data.Append("protected ");
                    if (field.IsAssembly)
                        data.Append("internal ");
                    if (field.IsFamilyOrAssembly)
                        data.Append("protected internal ");
                    if (field.IsStatic)
                        data.Append("static ");
                    if (field.IsInitOnly)
                        data.Append("readonly ");
                    if (field.IsLiteral)
                        data.Append("const ");

                     
                    data.Append(field.FieldType + " " + field.Name + Environment.NewLine + Environment.NewLine);
                }
                data.Append(Environment.NewLine + "\tСвойства " + Environment.NewLine);
                foreach (PropertyInfo property in researchType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    string propertyMod = null;
                    string getMethodMod = null;
                    string setMethodMod = null;
                    var getMethod = property.GetGetMethod(true);
                    var setMethod = property.GetSetMethod(true);
                    if (property.GetCustomAttributes().Count() > 0)
                        AddAttributes(property, data);

                    data.Append("\t\t");
                    if (getMethod != null && setMethod == null)
                        data.Append(GetVisibility(getMethod));
                    else if (setMethod != null && getMethod == null)
                        data.Append(GetVisibility(setMethod));
                    else
                    {
                        string getMod = GetVisibility(getMethod);
                        string setMod = GetVisibility(setMethod);
                        if (GetEq(getMod) > GetEq(setMod))
                        {
                            data.Append(getMod);
                            propertyMod = getMod;
                            setMethodMod = setMod;
                        }
                        else
                        {
                            data.Append(setMod);
                            propertyMod = setMod;
                            if (GetEq(getMod) != GetEq(setMod))
                                getMethodMod = getMod;
                        }

                        int GetEq(string mod)
                        {
                            switch (mod)
                            {
                                case "public ":
                                    return 5;
                                case "internal ":
                                    return 4;
                                case "protected internal ":
                                    return 3;
                                case "protected ":
                                    return 2;
                                case "private ":
                                    return 1;
                                default:
                                    return -1;
                            }
                        }
                    }


                    if (getMethod != null && getMethod.IsStatic || setMethod != null && setMethod.IsStatic)
                        data.Append("static ");
                    data.Append(property.PropertyType + " " + property.Name);
                    if(property.GetIndexParameters().Length > 0)
                    {
                        data.Append("[");
                        var indexParam = property.GetIndexParameters();
                        for (int i = 0; i < indexParam.Length; i++)
                        {
                            data.Append(indexParam[i].ParameterType + " " + indexParam[i].Name);
                            if (i + 1 < indexParam.Length)
                                data.Append(", ");
                        }
                        data.Append("] ");
                    }
                    data.Append(Environment.NewLine + "\t\t{" + Environment.NewLine); 
                    if (getMethod != null)
                    {
                        MethodsInPropNames?.Add("get_" + property.Name);
                        if(property.GetMethod.GetCustomAttributes().Count() > 0)
                            AddAttributes(property.GetMethod, data,"\t");
                        data.Append("\t\t\t");
                        if (getMethodMod != null)
                            data.Append(getMethodMod + " ");
                        data.Append("get;" + Environment.NewLine);
                    }
                        
                    if(setMethod != null)
                    {
                        MethodsInPropNames?.Add("set_" + property.Name);
                        if (property.SetMethod.GetCustomAttributes().Count() > 0)
                            AddAttributes(property.SetMethod, data,"\t");
                        data.Append("\t\t\t");
                        if (setMethodMod != null)
                            data.Append(setMethodMod + " ");
                        data.Append("set;" + Environment.NewLine);
                    }
                    data.Append("\t\t}" + Environment.NewLine + Environment.NewLine);
                }
                
                data.Append(Environment.NewLine + "\tМетоды" + Environment.NewLine);
                foreach (var method in researchType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    if((MethodsInPropNames != null && !MethodsInPropNames.Contains(method.Name) && !MethodsInPropNames.Contains("get_" + method.Name.Replace("get_",""))
                        && !MethodsInPropNames.Contains("set_" + method.Name.Replace("set_", ""))) || MethodsInPropNames == null)
                    {
                        if (method.GetCustomAttributes().Count() > 0)
                            AddAttributes(method, data);
                        data.Append("\t\t");
                        if (method.IsPublic)
                            data.Append("public ");
                        if (method.IsPrivate)
                            data.Append("private ");
                        if (method.IsFamily)
                            data.Append("protected ");
                        if (method.IsAssembly)
                            data.Append("internal ");
                        if (method.IsFamilyOrAssembly)
                            data.Append("protected internal ");
                        if (method.IsStatic)
                            data.Append("static ");
                        if (method.IsAbstract)
                            data.Append("abstract ");
                        if (researchType.BaseType != null)
                        {
                            if (method.IsVirtual && !method.IsFinal && GetBaseMethod(method) == null)
                                data.Append("virtual ");
                            if (method.IsVirtual && GetBaseMethod(method) != null && GetBaseMethod(method).IsVirtual &&
                               !GetBaseMethod(method).IsFinal)
                            {
                                data.Append("override ");
                                if (method.IsFinal)
                                    data.Append("sealed ");
                            }
                        }
                        else
                        {
                            if (method.IsVirtual)
                                data.Append("virtual ");
                        }

                        data.Append(method.ReturnType + " " + method.Name);
                        if(method.IsGenericMethod)
                        {
                            data.Append(AddGenericTypes(method));
                        }
                        data.Append(" (");
                        var parameters = method.GetParameters();
                        if (parameters.Length > 0)
                        {
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                if (parameters[i].IsOut) //не сделано для ref, params
                                    data.Append("out ");
                                data.Append(parameters[i].ParameterType.Name + " " + parameters[i].Name);
                                if (i + 1 < parameters.Length) data.Append(", ");
                            }
                        }
                        data.Append(")" + Environment.NewLine + Environment.NewLine);
                    }
                }
                data.Append(Environment.NewLine + "\tКонструкторы" + Environment.NewLine);
                foreach (ConstructorInfo constructor in researchType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    data.Append("\t\t");
                    if (constructor.IsAbstract)
                        data.Append("abstract ");
                    if (constructor.IsPublic)
                        data.Append("public ");
                    if (constructor.IsPrivate)
                        data.Append("private ");
                    if (constructor.IsFamily)
                        data.Append("protected ");
                    if (constructor.IsAssembly)
                        data.Append("internal ");
                    if (constructor.IsFamilyOrAssembly)
                        data.Append("protected internal ");
                    if (constructor.IsStatic)
                        data.Append("static ");

                    data.Append(researchType.Name + "(");
                    var parameters = constructor.GetParameters();
                    if (parameters.Length > 0)
                    {
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            data.Append(parameters[i].ParameterType.Name + " " + parameters[i].Name);
                            if (i + 1 < parameters.Length) data.Append(", ");
                        }
                    }
                    data.Append(")" + Environment.NewLine);
                }

                data.Append(Environment.NewLine + "\tСобытия" + Environment.NewLine);
                foreach (EventInfo @event in researchType.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    data.Append("\t\t" + @event.EventHandlerType + " " + @event.Name + Environment.NewLine);
                }
                data.Append("}");
                text.Text = data.ToString();


                string GetVisibility(MethodInfo m)
                {
                    string visibility = "";
                    if (m.IsPublic)
                        return "public ";
                    else if (m.IsPrivate)
                        return "private ";
                    else if (m.IsFamily)
                        visibility = "protected ";
                    else if (m.IsAssembly)
                        visibility = "internal ";
                    else if (m.IsFamilyOrAssembly)
                        visibility = "protected internal ";
                    return visibility;
                }
                bool IsSameParameters(MethodInfo method)
                {
                    if (researchType.BaseType == null)
                        throw null;
                    var currentParameters = method.GetParameters();

                    var baseParameters = researchType.BaseType.GetMethods().Where(m => m.Name == method.Name && m.ReturnType == method.ReturnType
                     && m.GetParameters().Length == method.GetParameters().Length).FirstOrDefault()?.GetParameters();
                    if (baseParameters == null)
                        return false;
                    for (int i = 0; i < method.GetParameters().Length; i++)
                    {
                        if (currentParameters[i].ParameterType != baseParameters[i].ParameterType)
                            return false;
                    }
                    return true;
                }
                MethodInfo GetBaseMethod(MethodInfo method)
                {
                    return researchType.BaseType.GetMethods().Where(m => m.Name == method.Name && m.ReturnType == method.ReturnType
                        && IsSameParameters(method)).FirstOrDefault();
                }
            }
        }

        /// <summary>Добавляет данные о типах-generic для исследуемого типа.</summary>
        /// <param name="type">Исследуемый тип.</param>
        /// <returns>Строка данных о типах-generic.</returns>
        private string AddGenericTypes(Type type)
        {
            StringBuilder data = new StringBuilder();
            int i = 0;
            data.Append("<");
            System.Type[] types = type.GetGenericArguments();
            foreach (System.Type Subtype in types)
            {
                data.Append(Subtype.Name);
                if (++i < types.Length)
                    data.Append(", ");
            }
            data.Append(">");
            return data.ToString();
        }
        /// <summary>Добавляет данные о типах-generic для исследуемого метода.</summary>
        /// <param name="method">Исследуемый метод.</param>
        /// <returns>Строка данных о типах-generic.</returns>
        /// <overloads>Добавляет данные о типах-generic.</overloads>
        private string AddGenericTypes(MethodInfo method)
        {
            StringBuilder data = new StringBuilder();
            int i = 0;
            data.Append("<");
            System.Type[] types = method.GetGenericArguments();
            foreach (System.Type Subtype in types)
            {
                data.Append(Subtype.Name);
                if (++i < types.Length)
                    data.Append(", ");
            }
            data.Append(">");
            return data.ToString();
        }
        /// <summary>Добавляет аттрибуты для исследуемого типа в строку data.</summary>
        /// <param name="type">Исследуемый тип.</param>
        /// <param name="data">Строка, куда будет добавлена информация.</param>
        /// <overloads>Добавляет аттрибуты во входную строку</overloads>
        private void AddAttributes(Type type, StringBuilder data)
        {
            foreach (var atrib in type.CustomAttributes)
            {
                data.Append("[" + atrib.AttributeType.FullName.Replace("Attribute", ""));
                if (atrib.ConstructorArguments.Count > 0)
                {
                    data.Append("(");
                    for (int i = 0; i < atrib.ConstructorArguments.Count; i++)
                    {
                        data.Append(atrib.ConstructorArguments[i].ArgumentType.Name + ": " + atrib.ConstructorArguments[i].Value);
                        if (i + 1 < atrib.ConstructorArguments.Count)
                            data.Append(",");
                    }
                    data.Append(")");
                }

                data.Append("]" + Environment.NewLine);
            }
        }
        /// <summary>Добавляет аттрибуты для исследуемого члена в строку data.</summary>
        /// <param name="member">Исследуемый член.</param>
        /// <param name="data">Строка, куда будет добавлена информация.</param>
        private void AddAttributes(MemberInfo member, StringBuilder data) //выводит не все атрибуты, см. строку индексатор или например класс task
        {
            foreach (var atrib in member.CustomAttributes)
            {
                data.Append("\t\t[" + atrib.AttributeType.FullName.Replace("Attribute", ""));
                if (atrib.ConstructorArguments.Count > 0)
                {
                    data.Append("(");
                    for (int i = 0; i < atrib.ConstructorArguments.Count; i++)
                    {
                        data.Append(atrib.ConstructorArguments[i].ArgumentType.Name + ": " + atrib.ConstructorArguments[i].Value);
                        if (i + 1 < atrib.ConstructorArguments.Count)
                            data.Append(",");
                    }
                    data.Append(")");
                }

                data.Append("]" + Environment.NewLine);
            }
        }

        /// <summary>Добавляет аттрибуты для исследуемого члена в строку data с отступом indent.</summary>
        /// <param name="member">Исследуемый член.</param>
        /// <param name="data">Строка, куда будет добавлена информация.</param>
        /// <param name="indent">Строка отступа.</param>
        private void AddAttributes(MemberInfo member, StringBuilder data, string indent)
        {
            foreach (var atrib in member.CustomAttributes)
            {
                data.Append(indent + "\t\t[" + atrib.AttributeType.FullName.Replace("Attribute", ""));
                if (atrib.ConstructorArguments.Count > 0)
                {
                    data.Append("(");
                    for (int i = 0; i < atrib.ConstructorArguments.Count; i++)
                    {
                        data.Append(atrib.ConstructorArguments[i].ArgumentType.Name + ": " + atrib.ConstructorArguments[i].Value);
                        if (i + 1 < atrib.ConstructorArguments.Count)
                            data.Append(",");
                    }
                    data.Append(")");
                }

                data.Append("]" + Environment.NewLine);
            }
        }

    }
}
