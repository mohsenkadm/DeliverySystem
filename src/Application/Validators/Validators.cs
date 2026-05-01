using DeliverySystem.Application.DTOs;
using FluentValidation;

namespace DeliverySystem.Application.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("اسم العميل مطلوب").MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().WithMessage("رقم الهاتف مطلوب").Matches(@"^[0-9+\-\s]+$").WithMessage("رقم هاتف غير صحيح");
        RuleFor(x => x.Username).NotEmpty().WithMessage("اسم المستخدم مطلوب").MinimumLength(3).MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty().WithMessage("كلمة المرور مطلوبة").MinimumLength(6).WithMessage("كلمة المرور يجب أن تكون 6 أحرف على الأقل");
        RuleFor(x => x.Address).NotEmpty().WithMessage("العنوان مطلوب");
    }
}

public class CreateAdminValidator : AbstractValidator<CreateAdminDto>
{
    public CreateAdminValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("الاسم الكامل مطلوب");
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeDto>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("اسم الموظف مطلوب").MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().WithMessage("رقم الهاتف مطلوب");
        RuleFor(x => x.Username).NotEmpty().WithMessage("اسم المستخدم مطلوب").MinimumLength(3);
        RuleFor(x => x.Password).NotEmpty().WithMessage("كلمة المرور مطلوبة").MinimumLength(6);
    }
}

public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("اسم التصنيف مطلوب").MaximumLength(100);
    }
}

public class CreateWarehouseValidator : AbstractValidator<CreateWarehouseDto>
{
    public CreateWarehouseValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("اسم المستودع مطلوب").MaximumLength(100);
    }
}

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("اسم المنتج مطلوب");
        RuleFor(x => x.RetailPrice).GreaterThanOrEqualTo(0).WithMessage("سعر المفرق يجب أن يكون صفراً أو أكبر");
        RuleFor(x => x.WholesalePrice).GreaterThanOrEqualTo(0).WithMessage("سعر الجملة يجب أن يكون صفراً أو أكبر");
        RuleFor(x => x.DiscountPercentage).InclusiveBetween(0, 100).WithMessage("نسبة الخصم يجب أن تكون بين 0 و 100");
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("التصنيف مطلوب");
    }
}

public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceDto>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("العميل مطلوب");
        RuleFor(x => x.Details).NotEmpty().WithMessage("يجب إضافة منتج واحد على الأقل");
        RuleForEach(x => x.Details).ChildRules(d =>
        {
            d.RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("المنتج مطلوب");
            d.RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("الكمية يجب أن تكون أكبر من صفر");
            d.RuleFor(x => x.UnitPrice).GreaterThan(0).WithMessage("السعر يجب أن يكون أكبر من صفر");
        });
    }
}

public class LoginValidator : AbstractValidator<AdminLoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("اسم المستخدم مطلوب");
        RuleFor(x => x.Password).NotEmpty().WithMessage("كلمة المرور مطلوبة");
    }
}
