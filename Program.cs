/*   1. Дан класс (ниже),
создать методы создающие этот класс, вызывая один из его конструкторов
(по одному конструктору на метод)
     2. Напишите 2 метода использующие рефлексию
1 - сохраняет информацию о классе в строку
2- позволяет восстановить класс из строки с информацией о методе
В качестве примере класса используйте класс TestClass.
Шаблоны методов для реализации:
static object StringToObject(string s) { }
static string ObjectToString(object o) { }
Подсказка:
Строка должна содержать название класса, полей и значений
Ограничьтесь диапазоном значений представленном в классе
Если класс находится в той же сборке (наш вариант) то можно не указывать имя сборки в паремтрах активатора.
Activator.CreateInstance(null, “TestClass”) - сработает;
Для простоты представьте что есть только свойства. Не анализируйте поля класса.
Пример того как мог быть выглядеть сохраненный в строку объект:
“TestClass, test2, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null:TestClass | I:1 | S:STR | D:2.0 |”
Ключ - значения разделяются двоеточием а сами пары - вертикальной чертой.
------------------------------------------------------------------------------------------------------------------
    3. Домашнее задание: 
Разработайте атрибут, позволяющий методу ObjectToString сохранять поля классов с использованием произвольного имени. 
Метод StringToObject должен также уметь работать с этим атрибутом для записи значения в свойство по имени его атрибута.
[CustomName(“CustomFieldName”)]
public int I = 0.
Если использовать формат строки с данными, использованными нами для предыдущего примера,
то пара ключ - значение для свойства I выглядела бы CustomFieldName:0
Подсказка: 
Если GetProperty(propertyName) вернул null то очевидно свойства с таким именем нет и возможно имя является алиасом,
заданным с помощью CustomName. Возможно, если перебрать все поля с таким атрибутом,
то для одного из них propertyName = совпадает с таковым заданным атрибутом.
 */

using System;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;

namespace Reflexia1
{
    internal class Program
    {
        public static TestClass MakeTestClass()
        {
            var typeTestClass = typeof(TestClass);
            return (TestClass)Activator.CreateInstance(typeTestClass);
        }
        public static TestClass MakeTestClass(int i)
        {
            var typeTestClass = typeof(TestClass);
            // Это конструктор private, поэтому сначала ищем приватный конструктор с нужной сигнатурой:
            var ctorPrivate = typeTestClass.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(int) }, null);
            // Если нашли - создаём экземпляр класса:
            if (ctorPrivate != null)
            {
                return (TestClass)ctorPrivate.Invoke(new Object[] { i });
            }
            else
            {
                throw new InvalidOperationException("Конструктор с параметром int не найден.");
            }
        }
        public static TestClass MakeTestClass(int i, string s, decimal d, char[] c)
        {
            var typeTestClass = typeof(TestClass);
            // Получаем массив всех публичных конструкторов
            var allCtors = typeTestClass.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            Console.WriteLine($"Количество конструкторов: {allCtors.Length}");
            // Исследуем все конструкторы:
            foreach (var ctor in allCtors)
            {
                Console.WriteLine($"Обрабатываем конструктор - {ctor}");
                // Получаем параметры конструктора
                ParameterInfo[] parameters = ctor.GetParameters();
                foreach (var param in parameters)
                {
                    Console.WriteLine($"  Параметр: {param.Name}, Тип: {param.ParameterType}");
                }
                // Если нашли конструктор с нужной сигнатурой - создаём экземпляр
                if (parameters.Length == 4 && parameters[0].ParameterType == typeof(int) && parameters[1].ParameterType == typeof(string)
                    && parameters[2].ParameterType == typeof(decimal) && parameters[3].ParameterType == typeof(char[]))
                {
                    return (TestClass)ctor.Invoke(new Object[] { i, s, d, c });
                }
            }
            throw new InvalidOperationException("Конструктор с данным набором параметров не найден.");
        }

        // Задание 2 + Домашнее задание
        public static object StringToObject(string s)
        {
            string[] arr = s.Split(" | ");
            string[] arrAssemblyQualifiedName = arr[0].Split(" : ");
            string[] arrName = arrAssemblyQualifiedName[0].Split(", ");
            var newInstance = Activator.CreateInstance(null, arrName[0]).Unwrap();
            if (arr.Length > 1 && newInstance != null)
            {
                Type type = newInstance.GetType();
                for (int i = 1; i < arr.Length - 1; i++)
                {
                    string namePropertyOrAlias = arr[i].Split(" : ")[0];  // Выделяем название совйства или его алиас
                    string valueProperty = arr[i].Split(" : ")[1]; // Выделяем значение этого свойства
                    if (type.GetProperty(namePropertyOrAlias) == null) // Использован Алиас вместо Имени свойства
                        namePropertyOrAlias = AttributeHelper.GetPropertyByAttributeName<TestClass>(namePropertyOrAlias);
                    PropertyInfo propertyInfo = type.GetProperty(namePropertyOrAlias); // Получаем PropertyInfo для свойства
                    Type propertyType = propertyInfo.PropertyType; // Получаем тип совйства
                    if (propertyType != typeof(char[]))
                    {
                        object convertedValue = Convert.ChangeType(valueProperty, propertyType); // Преобразуем значение свойства в нужный тип, если возможно
                        propertyInfo.SetValue(newInstance, convertedValue);
                    }
                    else
                        propertyInfo.SetValue(newInstance, valueProperty.ToCharArray());
                }
            }
            return newInstance;
        }
        public static string ObjectToString(object o)
        {
            Type typeObject = o.GetType();
            StringBuilder str = new StringBuilder();
            str.Append(typeObject.AssemblyQualifiedName + " : ");
            //str.Append(typeObject.FullName + " : ");
            str.Append(typeObject.Name + " | ");
            var properties = typeObject.GetProperties();
            string propertyNameOrAlias; // Храним имя свойства или имя атрибута, если он существует у совойства.
            foreach (var prop in properties)
            {
                if (Attribute.IsDefined(prop, typeof(CastomNameAttribute)))
                    propertyNameOrAlias = ((CastomNameAttribute)prop.GetCustomAttribute(typeof(CastomNameAttribute), false)).Name;
                else
                    propertyNameOrAlias = prop.Name;

                if (prop.PropertyType == typeof(char[]))
                    str.Append(propertyNameOrAlias + " : " + new string(prop.GetValue(o) as char[]) + " | ");
                else
                    str.Append(propertyNameOrAlias + " : " + prop.GetValue(o) + " | ");
            }
            return str.ToString();
        }

        static void Main(string[] args)
        {

            // Задание 1.
            Console.WriteLine("\nЗадание 1\n");
            // -- 1 --

            Console.WriteLine("Создаём объект класса по его типу через конструктор по умолчанию:");
            TestClass instance_1_OfTestClass = MakeTestClass();
            Console.WriteLine(instance_1_OfTestClass);

            // -- 2 --

            Console.WriteLine("\nСоздаём объект класса по его типу через конструктор с одним параметром:");
            // Это конструктор private, поэтому сначала ищем приватный конструктор с нужной сигнатурой:
            TestClass instance_2_OfTestClass = MakeTestClass(10);
            Console.WriteLine(instance_2_OfTestClass);
            // -- 3 --

            Console.WriteLine("\nСоздаём объект класса по его типу через конструктор со всеми параметрами," +
                " в том числе и с одним приватным:");
            TestClass instance_3_OfTestClass = MakeTestClass(20, "Инициированы все поля", 4.56M, new char[] { 'a', 'b', 'c' });
            Console.WriteLine(instance_3_OfTestClass);

            // Задание 2.
            Console.WriteLine(new string('-', 80));
            Console.WriteLine("\nЗадание 2\n");
            string myClass_string = ObjectToString(instance_3_OfTestClass);
            Console.WriteLine(myClass_string + "\n");
            var myClass_object = StringToObject(myClass_string);
            Console.WriteLine($"Новый экземпляр класса: {myClass_object}");

            // Домашнее Задание
            Console.WriteLine(new string('-', 80));
            Console.WriteLine("\nДОМАШНЕЕ ЗАДАНИЕ");

            string dataForObj1 = "Reflexia1.TestClass, Reflexia1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null : TestClass |" +
                " IntegerProp : 20 | StringProp : Все поля объявлены через Алиасы | DecimalProp : 4,56 | CharArrayProp : abc | ";
            string dataForObj2 = "Reflexia1.TestClass, Reflexia1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null : TestClass |" +
                " I : 35 | S : Все поля объявлены через свойства | D : 78,56 | C : xyz | ";
            string dataForObj3 = "Reflexia1.TestClass, Reflexia1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null : TestClass |" +
                " IntegerProp : 145 | S : Часть полей объявлена через свойства, а часть через алиасы | D : 56,012 | CharArrayProp : Reflex | ";

            // #1
            Console.WriteLine("\n# 1");
            Console.WriteLine($"\nИсходная строка:\n{dataForObj1}");
            var myClass_object1 = StringToObject(dataForObj1);
            Console.WriteLine($"\nНовый экземпляр класса из этой строки:\n{myClass_object1}");
            Console.WriteLine("\nСтрока из этого объекта:");
            Console.WriteLine($"{ObjectToString(myClass_object1)}");

            // #2
            Console.WriteLine("\n# 2");
            Console.WriteLine($"\nИсходная строка:\n{dataForObj2}");
            var myClass_object2 = StringToObject(dataForObj2);
            Console.WriteLine($"\nНовый экземпляр класса из этой строки:\n{myClass_object2}");
            Console.WriteLine("\nСтрока из этого объекта:");
            Console.WriteLine($"{ObjectToString(myClass_object2)}");

            // #3
            Console.WriteLine("\n# 3");
            Console.WriteLine($"\nИсходная строка:\n{dataForObj3}");
            var myClass_object3 = StringToObject(dataForObj3);
            Console.WriteLine($"\nНовый экземпляр класса из этой строки:\n{myClass_object3}");
            Console.WriteLine("\nСтрока из этого объекта:");
            Console.WriteLine($"{ObjectToString(myClass_object3)}");
        }
    }
}

