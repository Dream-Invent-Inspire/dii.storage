using dii.storage.cosmos.tests.Attributes;
using dii.storage.cosmos.tests.DiiCosmosAdapterTests;
using dii.storage.cosmos.tests.Orderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace dii.storage.cosmos.tests.DiiCosmosContextTests
{

    [Collection(nameof(FetchApiTests))]
    [TestCollectionPriorityOrder(400)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class LookupTests
    {
        [Fact, TestPriorityOrder(101)]
        public void DynamicTypeTest()
        {
            quicktest();
        }

        private void quicktest()
        {
            var typeName = "PersonOrderLookup";

            var assemblyName = new AssemblyName("DynamicAssembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");

            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public, typeof(object), new[] { typeof(IDoh) });

            // Define a default constructor (a constructor that takes no parameters)
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);

            // Get the ILGenerator for the constructor
            var ilGenerator = constructorBuilder.GetILGenerator();

            // Load 'this' onto the evaluation stack
            ilGenerator.Emit(OpCodes.Ldarg_0);

            // Load the address of the base (System.Object) constructor
            var objectConstructor = typeof(object).GetConstructor(new Type[0]);
            ilGenerator.Emit(OpCodes.Call, objectConstructor);

            // Return from the constructor method
            ilGenerator.Emit(OpCodes.Ret);

            //These are for the interface implementations...handle separately
            DefineProperty(typeBuilder, "SchemaVersion", typeof(Version));




            var ttt = typeBuilder.CreateTypeInfo().AsType();


        }

        private static void DefineProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType, bool getterOnly = false)
        {
            var fieldBuilder = typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);

            var getMethodBuilder = typeBuilder.DefineMethod($"IDoh.get_{propertyName}", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Final, propertyType, Type.EmptyTypes);
            var getIL = getMethodBuilder.GetILGenerator();
            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Ret);

            MethodBuilder setMethodBuilder = null;
            if (!getterOnly)
            {
                setMethodBuilder = typeBuilder.DefineMethod($"IDoh.set_{propertyName}", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Final, null, new Type[] { propertyType });
                var setIL = setMethodBuilder.GetILGenerator();
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, fieldBuilder);
                setIL.Emit(OpCodes.Ret);
            }

            var propertyBuilder = typeBuilder.DefineProperty($"IDoh.{propertyName}", PropertyAttributes.None, propertyType, null);
            propertyBuilder.SetGetMethod(getMethodBuilder);
            if (!getterOnly && setMethodBuilder != null) propertyBuilder.SetSetMethod(setMethodBuilder);
        }



        //private static void DefineProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType, bool getterOnly = false)
        //{
        //    var fieldBuilder = typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);
        //    var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

        //    var getMethodBuilder = typeBuilder.DefineMethod($"get_{propertyName}", MethodAttributes.Public, propertyType, Type.EmptyTypes);
        //    var getIL = getMethodBuilder.GetILGenerator();
        //    getIL.Emit(OpCodes.Ldarg_0);
        //    getIL.Emit(OpCodes.Ldfld, fieldBuilder);
        //    getIL.Emit(OpCodes.Ret);

        //    propertyBuilder.SetGetMethod(getMethodBuilder);

        //    if (!getterOnly)
        //    {
        //        var setMethodBuilder = typeBuilder.DefineMethod($"set_{propertyName}", MethodAttributes.Public, null, new Type[] { propertyType });
        //        var setIL = setMethodBuilder.GetILGenerator();
        //        setIL.Emit(OpCodes.Ldarg_0);
        //        setIL.Emit(OpCodes.Ldarg_1);
        //        setIL.Emit(OpCodes.Stfld, fieldBuilder);
        //        setIL.Emit(OpCodes.Ret);

        //        propertyBuilder.SetSetMethod(setMethodBuilder);
        //    }
        //}

    }


    public interface IDoh
    {
        Version SchemaVersion { get; set; }
    }

}
