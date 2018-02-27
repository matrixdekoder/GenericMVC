using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMVC.Models
{
    public class Base
    {
        [Key()]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [NotListed]
        public int Id { get; set; }
    }

    [DisplayTableName(Name = "Author")]
    public class Author : Base
    {
        [Display(Name = "Name")]
        public string Name { get; set; }
        

        public override string ToString()
        {
            return $"{Name}";
        }
        
        [Display(Name = "Date of birth")]
        public DateTime BirthDate { get; set; }

        public ICollection<AuthorBook> AuthorBook { get; set; }
    }

    [DisplayTableName(Name = "Books")]
    public class Book : Base
    {
        public string Title { get; set; }
        public string Genre { get; set; }

        public ICollection<AuthorBook> Author { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }

    [DisplayTableName(Name = "Book's authors")]
    public class AuthorBook: Base
    {
        [ForeignKey("Author")]
        public int AuthorID { get; set; }

        public Author Author { get; set; }


        [ForeignKey("Book")]
        public int BookID { get; set; }
        public Book Book { get; set; }

        public override string ToString()
        {
            return $"{Book}-{Author}";
        }
    }

   
    public class LibraryContext: DbContext
    {
        private ModelBuilder mb;
        private Dictionary<string,object> list = new Dictionary<string, object>();

        public DbSet<Author> Author { get; set; }
        public DbSet<Book> Book { get; set; }
        public DbSet<AuthorBook> AuthorBook { get; set; }

        public LibraryContext():base()
        {

        }

        public LibraryContext(DbContextOptions options): base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            this.mb = modelBuilder;
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Author>().ToTable("Author");
            modelBuilder.Entity<Book>().ToTable("Book");
            modelBuilder.Entity<AuthorBook>().ToTable("AuthorBook");
        }

        /// <summary>
        /// Get a dictionary of a table values with the database Key property and Value as the representation string of the class
        /// </summary>
        /// <param name="type">Type of the requested Table</param>
        /// <returns></returns>
        internal List<KeyValuePair<object,string>> GetTable(Type type)
        {
            //Get the DbContext Type
            var ttype = GetType();
            //The DbContext properties
            var props = ttype.GetProperties().ToList();
            // The DbSet property with base type @type
            var prop = props.Where(i => i.PropertyType.GenericTypeArguments.Any()&&i.PropertyType.GenericTypeArguments.First() == type).FirstOrDefault();

            //The DbSet instance
            var pvalue = prop?.GetValue(this);

            // Dictionary to return
            var l = new Dictionary<object, string>();

            var pv = (IEnumerable<object>)pvalue;

            //The entity Key property
            var keyprop = type.GetProperties().First(i => i.CustomAttributes.Any(j => j.AttributeType == typeof(KeyAttribute)));
            
            //Fills the dictionary
            foreach (Base item in pv)
            {
                //with the key and the ToString() entity result
                l.Add(keyprop.GetValue(item), item.ToString());
            }
            return l.ToList();
        }
    }
}
