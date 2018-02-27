using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using GenericMVC.Models;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GenericMVC.Controllers
{
    public class GenericController<T> : BaseController where T : Base
    {
        public GenericController(LibraryContext context) : base(context)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Page index</param>
        /// <param name="id1">Items per page</param>
        /// <returns></returns>
        public IActionResult Index(int? id, int? id1)
        {
            var pagesize = id1 ?? 100;
            var pageindex = id ?? 1;
            var source = _context.Set<T>().OrderBy(i => i.Id);
            var data = source.Skip((pageindex-1)*pagesize).Take(pagesize).ToList();
            ViewBag.TotalPages = (int)source.Count() /pagesize + (source.Count() % pagesize==0?0:1);
            var type = typeof(T);
            var attr = type.CustomAttributes.Where(i => i.AttributeType == typeof(DisplayTableNameAttribute)).FirstOrDefault();
            ViewBag.TypeName = attr.NamedArguments.First().TypedValue.Value;
            var props = type.GetProperties().ToList().Where(i => !i.PropertyType.Name.StartsWith("ICollection")).ToList();
            var props1 = props.Where(i => i.CustomAttributes.Any(k => k.AttributeType == typeof(ForeignKeyAttribute))).ToList();
            props = props.Except(props1).ToList();
            props = props.Where(i => !i.CustomAttributes.Any(k => k.AttributeType == typeof(NotListedAttribute))).ToList();
            ViewBag.props = props;
            return View(data);
        }

        /// <summary>
        /// Filter by 
        /// </summary>
        /// <returns></returns>
        public IActionResult FilterBy()
        {
            var props = PrepareEditorView(null);
            ViewBag.props = props;
            return View();
        }

        [HttpPost]
        public IActionResult FilterBy(IFormCollection fm)
        {
            var props = PrepareEditorView(null);
            ViewBag.props = props;
            return View();
        }

        public IActionResult Create()
        {
            var props = PrepareEditorView(null);
            ViewBag.props = props.Where(i=>!i.CustomAttributes.Any(ia=>ia.AttributeType==typeof(KeyAttribute))).ToList();
            return View();
        }

        private List<PropertyInfo> PrepareEditorView(T model)
        {
            var type = typeof(T);
            var attr = type.CustomAttributes.Where(i => i.AttributeType == typeof(DisplayTableNameAttribute)).FirstOrDefault();
            ViewBag.TypeName = attr.NamedArguments.First().TypedValue.Value;
            //Do not load remote keys properties
            var props = type.GetProperties().ToList().Where(i => !i.PropertyType.Name.StartsWith("ICollection")).ToList();
            var props1 = props.Where(i => i.CustomAttributes.Any(k => k.AttributeType == typeof(ForeignKeyAttribute))).ToList();
            foreach (var item in props1)
            {
                var iattr = item.CustomAttributes.First(i => i.AttributeType == typeof(ForeignKeyAttribute));
                var field = iattr.ConstructorArguments.First().Value.ToString();
                var typex = props.First(i => i.Name == field);
                var typ = typex.PropertyType;
                var data = _context.GetTable(typ);
                object pd = null;
                if (model!=null)
                {
                    pd = item.GetValue(model);
                }
                if (item.PropertyType.Name.StartsWith("Nullable"))
                {
                    data.Add(new KeyValuePair<object,string>(null, "---SIN ESPECIFICAR---"));
                }
                ViewData[item.Name] = new SelectList(data, "Key", "Value", pd);
                props = props.Where(i => i.Name != field).ToList();
            }
            return props;
        }

        [HttpPost]
        public IActionResult Create(T model)
        {
            try
            {
                _context.Add<T>(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ee)
            {
                ViewBag.Exception = ee;
                var props = PrepareEditorView(model);
                ViewBag.props = props;
                return View();
            }
        }

        public IActionResult Edit(int id)
        {
            var model = _context.Find<T>(id);
            ViewBag.props = PrepareEditorView(model);
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(T model)
        {
            try
            {
                _context.Update<T>(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ee)
            {
                ViewBag.Exception = ee;
                var props = PrepareEditorView(model);
                ViewBag.props = props;
                return View();
            }
        }

        public IActionResult Delete(int id)
        {
            var element = _context.Find<T>(id);
            ViewBag.props = PrepareEditorView(element);
            return View(element);
        }

        [HttpPost]
        public IActionResult Delete(IFormCollection fm)
        {
            var element = _context.Find<T>(int.Parse(fm["id"]));
            _context.Remove<T>(element);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult ImportFromCSVWithHeaders()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ImportFromCSVWithHeaders(FormCollection fm)
        {
            var props = PrepareEditorView(null).ToDictionary(i=>i.Name,i=>i);
            var file = Request.Form.Files[0];
            var sr = new StreamReader(file.OpenReadStream());
            var lines = sr.ReadToEnd().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Where(i => string.IsNullOrWhiteSpace(i)).ToList();
            var headers = lines[0].Split(';').ToList();
            var data = lines.Skip(1).Select(i => i.Split(';')).ToList();

            var ctor = typeof(T).GetConstructors().First();
            foreach (var item in data)
            {
                T obj = (T)ctor.Invoke(null);
                foreach (var header in headers)
                {
                    if (!props.ContainsKey(header)) continue;
                    var svalue = item[headers.IndexOf(header)];
                    object valu = null;
                    switch (props[header].PropertyType.Name)
                    {
                        case "string":
                            {
                                valu = svalue;
                                break;
                            }
                        default:
                            {
                                var tp = props[header].PropertyType.GetMethods().Where(i => i.Name.Contains("TryParse")).FirstOrDefault();
                                if (tp == null) continue;
                                if (!(bool)tp.Invoke(null, new[] { svalue, valu }))
                                    continue;
                                break;
                            }
                    }
                    props[header].SetValue(obj, valu);
                }
                _context.Add<T>(obj);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
