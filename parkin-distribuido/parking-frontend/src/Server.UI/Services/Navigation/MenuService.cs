using CleanArchitecture.Blazor.Application.Common.Constants;
using CleanArchitecture.Blazor.Server.UI.Models.NavigationMenu;

namespace CleanArchitecture.Blazor.Server.UI.Services.Navigation;

public class MenuService : IMenuService
{
    private readonly List<MenuSectionModel> _features = new()
    {
        new MenuSectionModel
        {
            Title = "Principal",
            SectionItems = new List<MenuSectionItemModel>
            {
                new() { Title = "Inicio", Icon = Icons.Material.Filled.Home, Href = "/" },
                new()
                {
                    Title = "Parqueadero",
                    Icon = Icons.Material.Filled.LocalParking,
                    PageStatus = PageStatus.Completed,
                    IsParent = true,
                    MenuItems = new List<MenuSectionSubItemModel>
                    {
                        new() { Title = "Parqueo", Href = "/pages/tickets", PageStatus = PageStatus.Completed },
                        new() { Title = "Parqueo móvil", Href = "/pages/parking-mobile", PageStatus = PageStatus.Completed },
                        new() { Title = "Tarifas Parqueo", Href = "/tables/parking-rates", PageStatus = PageStatus.Completed },
                    }
                },
                new()
                {
                    Title = "Lavadero",
                    Icon = Icons.Material.Filled.LocalCarWash,
                    PageStatus = PageStatus.Completed,
                    IsParent = true,
                    MenuItems = new List<MenuSectionSubItemModel>
                    {
                        new() { Title = "Lavados", Href = "/pages/carwashes", PageStatus = PageStatus.Completed },
                        new() { Title = "Lavadores", Href = "/tables/wash-operators", PageStatus = PageStatus.Completed },
                        new() { Title = "Tarifas Lavado", Href = "/tables/wash-prices", PageStatus = PageStatus.Completed },
                    }
                },
                new()
                {
                    Title = "Mensualidades",
                    Icon = Icons.Material.Filled.CardMembership,
                    PageStatus = PageStatus.Completed,
                    IsParent = true,
                    MenuItems = new List<MenuSectionSubItemModel>
                    {
                        new() { Title = "Mensualidades", Href = "/pages/members", PageStatus = PageStatus.Completed },
                        new() { Title = "Tarifas Mensualidad", Href = "/tables/monthly-rates", PageStatus = PageStatus.Completed },
                    }
                },
                new()
                {
                    Title = "Reportes",
                    Icon = Icons.Material.Filled.Assessment,
                    PageStatus = PageStatus.Completed,
                    IsParent = true,
                    MenuItems = new List<MenuSectionSubItemModel>
                    {
                        new() { Title = "Reporte Mensual", Href = "/pages/reporte", PageStatus = PageStatus.Completed },
                        new() { Title = "Utilidad Anual", Href = "/pages/utilidad", PageStatus = PageStatus.Completed },
                        new() { Title = "Estatus de Turno", Href = "/pages/estatus", PageStatus = PageStatus.Completed },
                        new() { Title = "Arqueo de Caja", Href = "/pages/arqueo", PageStatus = PageStatus.Completed },
                        new() { Title = "Gastos", Href = "/pages/gastos", PageStatus = PageStatus.Completed },
                    }
                },
            }
        },
        new MenuSectionModel
        {
            Title = "",
            Roles = new[] { Roles.Admin },
            SectionItems = new List<MenuSectionItemModel>
            {
                new()
                {
                    Title = "Ajustes",
                    Icon = Icons.Material.Filled.Settings,
                    IsParent = true,
                    MenuItems = new List<MenuSectionSubItemModel>
                    {
                        new() { Title = "Parqueaderos", Href = "/pages/carparks", PageStatus = PageStatus.Completed },
                        new() { Title = "Impresora POS", Href = "/pages/ajustes/impresora", PageStatus = PageStatus.Completed },
                        new() { Title = "Tipos de Vehiculo", Href = "/pages/vehicle-types", PageStatus = PageStatus.Completed },
                        new() { Title = "Usuarios", Href = "/identity/users", PageStatus = PageStatus.Completed },
                        new() { Title = "Roles", Href = "/identity/roles", PageStatus = PageStatus.Completed },
                    }
                }
            }
        }
    };

    public IEnumerable<MenuSectionModel> Features => _features;
}
