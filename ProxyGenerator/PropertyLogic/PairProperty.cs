﻿using System;
using ProxyGenerator.G;

namespace ProxyGenerator.PropertyLogic
{
    internal class PairProperty
    {
        private readonly ProxySourceGenerator _sg;

        public string PropertyName
        {
            get;
            private set;
        }

        private PropertyDefinition _getProp;
        public PropertyDefinition GetProp
        {
            get
            {
                return _getProp;
            }

            set
            {
                if (value == null && _setProp == null)
                {
                    throw new ArgumentException("value == null && _setProp == null");
                }
                if (value != null && !value.IsGet)
                {
                    throw new ArgumentException("!value.IsGet");
                }
                if (value != null && _setProp != null && value.PropertyName != _setProp.PropertyName)
                {
                    throw new ArgumentException("value != null && _setProp != null && value.PropertyName != _setProp.PropertyName");
                }

                _getProp = value;
            }
        }

        private PropertyDefinition _setProp;
        public PropertyDefinition SetProp
        {
            get
            {
                return
                    _setProp;
            }

            set
            {
                if (_getProp == null && value == null)
                {
                    throw new ArgumentException("_getProp == null && value == null");
                }
                if (_getProp != null && !_getProp.IsGet)
                {
                    throw new ArgumentException("!_getProp.IsGet");
                }
                if (value.IsGet)
                {
                    throw new ArgumentException("value.IsGet");
                }
                if (_getProp != null && value != null && _getProp.PropertyName != value.PropertyName)
                {
                    throw new ArgumentException("_getProp != null && value != null && _getProp.PropertyName != value.PropertyName");
                }

                _setProp = value;
            }
        }

        public Type PropertyType
        {
            get
            {
                if (_getProp != null)
                {
                    return
                        _getProp.Method.ReturnType;
                }

                return
                    _setProp.Method.GetParameters()[0].ParameterType;
            }
        }

        public PairProperty(
            ProxySourceGenerator sg,
            string propertyName)
        {
            if (sg == null)
            {
                throw new ArgumentNullException("sg");
            }
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            _sg = sg;
            PropertyName = propertyName;
        }

        public string ToSourceCode()
        {
            //меняем название
            var p0 = Title.Replace(
                "{_Name_}",
                PropertyName);

            //меняем тип проперти
            var propType = this.PropertyType;
            var propTypeName = 
                propType != typeof(void)
                    ? _sg.ParameterTypeConverter(propType)
                    : "void";

            var p1 = p0.Replace(
                "{_Type_}",
                ProxySourceGenerator.FullNameConverter(propTypeName));

            //ищем геттер
            var p2 = p1;
            if (_getProp != null)
            {
                var g = Getter.Replace(
                    "{_Name_}",
                    PropertyName);

                p2 = p1.Replace(
                    "{_Get_}",
                    g);
            }
            else
            {
                p2 = p1.Replace(
                    "{_Get_}",
                    string.Empty);
            }
            

            //ищем сеттер
            var p3 = p2;
            if(_setProp != null)
            {
                var s = Setter.Replace(
                    "{_Name_}",
                    PropertyName);


                p3 = p2.Replace(
                    "{_Set_}",
                    s);
            }
            else
            {
                p3 = p2.Replace(
                    "{_Set_}",
                    string.Empty);
            }


            return p3;
        }


        private const string Getter = @"
                get
                {
                    return _wrappedObject.{_Name_};
                }
";

        private const string Setter = @"
                set
                {
                    _wrappedObject.{_Name_} = value;
                }
";

        private const string Title = @"
            public {_Type_} {_Name_}
            {
                {_Get_}
                {_Set_}
            }

";

    }
}
