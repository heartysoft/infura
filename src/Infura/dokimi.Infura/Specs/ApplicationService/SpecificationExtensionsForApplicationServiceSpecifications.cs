using dokimi.core.Specs.ApplicationService;

// ReSharper disable CheckNamespace

namespace dokimi.core
// ReSharper restore CheckNamespace
{

    namespace dokimi.core.Specs.ApplicationService
    {
        public static class SpecificationExtensionsForApplicationServiceSpecifications
        {
            public static ApplicationServiceSpecification.GivenContext ApplicationServiceSpecification<T>(this Specifications spec)
                where T : SpecificationCategory, new()
            {
                return global::dokimi.core.Specs.ApplicationService.ApplicationServiceSpecification.New(new T());
            }

            public static ApplicationServiceSpecification.GivenContext ApplicationServiceSpecification(this Specifications spec,
                string context, string category)
            {
                return
                    global::dokimi.core.Specs.ApplicationService.ApplicationServiceSpecification.New(
                        new SpecificationCategory(context, category));
            }

            public static ApplicationServiceSpecification.GivenContext ApplicationServiceSpecification(this Specifications spec,
                SpecificationCategory category)
            {
                return global::dokimi.core.Specs.ApplicationService.ApplicationServiceSpecification.New(category);
            }
        }

    }
}