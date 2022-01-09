using GenericAdding.Data.Context;
using GenericAdding.DTOs;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GenericAdding.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenericAddingController : ControllerBase
    {
        private readonly MainContext _context;

        public GenericAddingController(MainContext context)
        {
            _context = context;
        }

        [HttpPost]
        public ActionResult AddItem(ItemToAdd itemtoAdd)
        {
            string tableClassName = itemtoAdd.TableName.Singularize().Pascalize();

            //To Get the result DbSet as an exaple (_context.Products)
            Type tableType = Type.GetType($"GenericAdding.Data.Models.{tableClassName}, GenericAdding");
            Type contextType = typeof(MainContext);
            var setMethod = contextType.GetMethod("Set", new Type[] { });
            var genricSetMethod = setMethod.MakeGenericMethod(tableType);
            var resultList = genricSetMethod.Invoke(_context, null);

            //To Get the Add Method as an example (_context.Products.Add) => without calling it 
            Type dbSetType = typeof(DbSet<>);
            var dbSetGenericType = dbSetType.MakeGenericType(new[] { tableType });
            var addMethod = dbSetGenericType.GetMethod("Add");

            //Getting the entity to add ready as an example (var entityToAdd = new Product{Name= "", InstallationFees=500})
            var entityToAdd = Activator.CreateInstance(tableType);
            itemtoAdd.Fields.ForEach(f =>
            {
                var prop = tableType.GetProperty(f.Name.Pascalize());
                var fieldType = prop.PropertyType;
                var fieldConvertedValue = Convert.ChangeType(f.Value, fieldType);
                prop.SetValue(entityToAdd, fieldConvertedValue, null);
            });

            //Calling Add method
            addMethod.Invoke(resultList, new[] { entityToAdd });

            //Calling Save Changes Method
            var saveChangesMethod = contextType.GetMethod("SaveChanges", new Type[] { });
            saveChangesMethod.Invoke(_context, null);

            //Or Simply
            _context.SaveChanges();

            return Ok();
        }
    }
}
