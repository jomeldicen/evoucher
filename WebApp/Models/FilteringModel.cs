using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace WebApp.Models
{
    public class FilterModel
    {
        const int maxPageSize = 100;

        public int page { get; set; } = 1;

        public int _pageSize { get; set; } = 10;

        public List<int> searchbyids { get; set; }

        public string sortby { get; set; }

        public string search { get; set; }

        public string PageUrl { get; set; }

        public List<string> multiplesearch { get; set; }

        public bool reverse { get; set; } = false;

        public string value1 { get; set; }
        public string value2 { get; set; }
        public string value3 { get; set; }
        public string value4 { get; set; }
        public string value5 { get; set; }

        public int itemsPerPage
        {
            get { return _pageSize; }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }
    }

    public class SearchModel
    {
        public string search { get; set; }
        public List<string> multiplesearch { get; set; }
    }
        
    public class ControllerParam
    {
        public string param1 { get; set; }
        public string param2 { get; set; }
        public string param3 { get; set; }
        public string param4 { get; set; }
        public string param5 { get; set; }
        public string param6 { get; set; }
        public string param7 { get; set; }

        public DateTime dt1 { get; set; }
        public DateTime dt2 { get; set; }
    }
}