using AutoSalonGrida.Models;
using AutoSalonGrida.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoSalonGrida.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        foreach (var role in new[] { "Администратор", "Пользователь", "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        const string adminEmail = "admin@autosalon.local";
        const string adminPassword = "Admin123!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Главный администратор",
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        if (!await context.Cars.AnyAsync())
        {
            var carsData = GetCarsData();
            var cars = carsData.Select((data, idx) => new Car
            {
                Brand = data.Brand,
                Model = data.Model,
                Year = data.Year,
                Price = data.Price,
                CategoryId = data.CategoryId,
                BodyType = data.BodyType,
                Description = data.Description,
                Mileage = data.Mileage,
                EngineType = data.EngineType,
                ImageUrl = string.Empty,
                PopularityScore = 100 - idx
            }).ToList();

            await context.Cars.AddRangeAsync(cars);
            await context.SaveChangesAsync();

            var photoService = serviceProvider.GetRequiredService<ICarPhotoService>();
            foreach (var car in cars)
            {
                var photos = await photoService.GetPhotosAsync(car.Brand, car.Model, 4);
                car.ImageUrl = photos[0];

                foreach (var galleryPhoto in photos.Skip(1))
                {
                    context.CarImages.Add(new CarImage { CarId = car.Id, ImagePath = galleryPhoto });
                }
            }

            await context.SaveChangesAsync();
        }

        var carPhotoService = serviceProvider.GetRequiredService<ICarPhotoService>();
        var existingCars = await context.Cars
            .Include(c => c.Images)
            .ToListAsync();

        foreach (var car in existingCars)
        {
            var hasLegacyMainImage = string.IsNullOrWhiteSpace(car.ImageUrl)
                || !car.ImageUrl.Contains("/images/cars/", StringComparison.OrdinalIgnoreCase);

            var shouldRefreshGallery = car.Images.Count < 3
                || car.Images.Any(i => !i.ImagePath.Contains("/images/cars/", StringComparison.OrdinalIgnoreCase));

            if (!hasLegacyMainImage && !shouldRefreshGallery)
            {
                continue;
            }

            var photos = await carPhotoService.GetPhotosAsync(car.Brand, car.Model, 4);
            car.ImageUrl = photos[0];

            context.CarImages.RemoveRange(car.Images);
            foreach (var galleryPhoto in photos.Skip(1))
            {
                context.CarImages.Add(new CarImage { CarId = car.Id, ImagePath = galleryPhoto });
            }
        }

        await context.SaveChangesAsync();
    }

    private static List<CarSeedItem> GetCarsData() =>
    [
        new("BMW", "X5", 2023, 7800000, 1, "SUV", "3.0 Turbo", 19000, "Комфортный и динамичный кроссовер для города и трассы."),
        new("BMW", "3 Series", 2022, 5300000, 2, "Седан", "2.0 Turbo", 25000, "Легендарный седан с отличной управляемостью."),
        new("Audi", "Q7", 2024, 9200000, 1, "SUV", "3.0 TFSI", 9000, "Семейный SUV премиум-класса."),
        new("Audi", "A6", 2023, 6100000, 2, "Седан", "2.0 TFSI", 17000, "Бизнес-седан с богатым оснащением."),
        new("Mercedes-Benz", "GLE", 2023, 9600000, 1, "SUV", "3.0 Turbo", 14000, "Статусный SUV с высоким уровнем безопасности."),
        new("Mercedes-Benz", "C-Class", 2022, 5600000, 2, "Седан", "2.0 Turbo", 23000, "Классика немецкого премиума."),
        new("Toyota", "Land Cruiser 300", 2024, 10500000, 1, "SUV", "3.5 Twin Turbo", 5000, "Надежный внедорожник для любых условий."),
        new("Toyota", "Camry", 2023, 3900000, 2, "Седан", "2.5", 18000, "Популярный и практичный бизнес-седан."),
        new("Honda", "CR-V", 2022, 4100000, 1, "SUV", "2.0 Hybrid", 27000, "Экономичный и просторный городской SUV."),
        new("Honda", "Civic", 2023, 3200000, 3, "Хэтчбек", "1.5 Turbo", 12000, "Спортивный характер и надежность Honda."),
        new("Ford", "Explorer", 2021, 4300000, 1, "SUV", "2.3 EcoBoost", 36000, "Большой семейный внедорожник."),
        new("Ford", "Mustang", 2022, 6200000, 7, "Купе", "5.0 V8", 21000, "Культовый американский спорткар."),
        new("Chevrolet", "Tahoe", 2022, 7100000, 1, "SUV", "5.3 V8", 28000, "Мощный SUV для дальних поездок."),
        new("Chevrolet", "Malibu", 2021, 2700000, 2, "Седан", "1.5 Turbo", 41000, "Комфортный седан для повседневной езды."),
        new("Tesla", "Model S", 2023, 9800000, 5, "Седан", "Электро", 10000, "Флагманский электромобиль с отличной динамикой."),
        new("Tesla", "Model 3", 2022, 6200000, 5, "Седан", "Электро", 19000, "Технологичный электромобиль для города."),
        new("Nissan", "X-Trail", 2023, 3800000, 1, "SUV", "2.5", 17000, "Практичный кроссовер для семьи."),
        new("Nissan", "Leaf", 2021, 2400000, 5, "Хэтчбек", "Электро", 33000, "Доступный и надежный электромобиль."),
        new("Hyundai", "Santa Fe", 2022, 4100000, 1, "SUV", "2.2 Diesel", 26000, "Современный и просторный SUV."),
        new("Hyundai", "Sonata", 2023, 3400000, 2, "Седан", "2.5", 15000, "Стильный седан с богатой комплектацией."),
        new("Kia", "Sorento", 2023, 4200000, 1, "SUV", "2.5", 16000, "Комфортный кроссовер для всей семьи."),
        new("Kia", "K5", 2022, 3100000, 2, "Седан", "2.0", 23000, "Современный седан со спортивным дизайном."),
        new("Volkswagen", "Touareg", 2022, 6800000, 1, "SUV", "3.0 TDI", 22000, "Немецкий SUV с отличной шумоизоляцией."),
        new("Volkswagen", "Golf", 2023, 2900000, 3, "Хэтчбек", "1.4 TSI", 11000, "Идеальный городской хэтчбек."),
        new("Lexus", "RX", 2023, 7600000, 8, "SUV", "2.5 Hybrid", 14000, "Премиальный комфорт и надежность Lexus."),
        new("Lexus", "ES", 2022, 5900000, 8, "Седан", "2.5", 24000, "Роскошный седан для деловых поездок."),
        new("Mazda", "CX-5", 2023, 3700000, 1, "SUV", "2.5", 13000, "Динамичный дизайн и отличная управляемость."),
        new("Mazda", "6", 2021, 3000000, 2, "Седан", "2.0", 36000, "Элегантный седан с японским качеством."),
        new("Subaru", "Forester", 2022, 3500000, 1, "SUV", "2.0", 29000, "Полный привод и высокий клиренс."),
        new("Subaru", "BRZ", 2023, 3900000, 7, "Купе", "2.4", 9000, "Легкое спортивное купе для драйва."),
        new("Volvo", "XC90", 2023, 8200000, 8, "SUV", "2.0 Hybrid", 12000, "Безопасный премиальный SUV."),
        new("Volvo", "S60", 2022, 4900000, 8, "Седан", "2.0", 22000, "Скандинавский стиль и высокий комфорт."),
        new("Porsche", "Cayenne", 2023, 11800000, 8, "SUV", "3.0 Turbo", 10000, "Премиальный спорт-SUV."),
        new("Porsche", "911 Carrera", 2022, 14500000, 7, "Купе", "3.0 Turbo", 8000, "Икона спортивных автомобилей."),
        new("Ferrari", "F8 Tributo", 2021, 34000000, 7, "Купе", "3.9 V8", 7000, "Суперкар с невероятной динамикой."),
        new("Ferrari", "Roma", 2022, 29500000, 7, "Купе", "3.9 V8", 6000, "Роскошный гран-туризмо Ferrari."),
        new("Lamborghini", "Huracan", 2022, 32000000, 7, "Купе", "5.2 V10", 5000, "Яркий и экстремальный суперкар."),
        new("Lamborghini", "Urus", 2023, 36000000, 8, "SUV", "4.0 V8", 4000, "Самый быстрый люксовый SUV."),
        new("Bentley", "Bentayga", 2022, 28500000, 8, "SUV", "4.0 V8", 11000, "Люксовый внедорожник ручной сборки."),
        new("Bentley", "Flying Spur", 2021, 26000000, 8, "Седан", "6.0 W12", 13000, "Роскошный седан представительского класса."),
        new("Rolls-Royce", "Cullinan", 2023, 52000000, 8, "SUV", "6.75 V12", 3000, "Эталон роскоши среди внедорожников."),
        new("Rolls-Royce", "Ghost", 2022, 47000000, 8, "Седан", "6.75 V12", 4500, "Премиальный седан абсолютного уровня."),
        new("BMW", "i4", 2023, 6900000, 5, "Седан", "Электро", 9000, "Современный электроседан BMW."),
        new("Audi", "e-tron GT", 2023, 11200000, 5, "Купе", "Электро", 7000, "Электроспорткар с премиальным салоном."),
        new("Mercedes-Benz", "EQE", 2024, 9800000, 5, "Седан", "Электро", 2000, "Инновационный электроседан Mercedes."),
        new("Toyota", "Prius", 2022, 3400000, 6, "Хэтчбек", "1.8 Hybrid", 21000, "Легендарный гибрид для города."),
        new("Honda", "Accord Hybrid", 2023, 3700000, 6, "Седан", "2.0 Hybrid", 12000, "Экономичный гибридный седан."),
        new("Hyundai", "Ioniq 5", 2023, 5200000, 5, "SUV", "Электро", 11000, "Футуристичный электрокроссовер."),
        new("Kia", "EV6", 2023, 5400000, 5, "Купе", "Электро", 9000, "Динамичный электрический кроссовер-купе."),
        new("Volkswagen", "ID.4", 2022, 4700000, 5, "SUV", "Электро", 15000, "Практичный электромобиль для семьи."),
        new("Nissan", "Qashqai", 2023, 3300000, 1, "SUV", "1.3 Turbo", 13000, "Популярный городской кроссовер."),
        new("Renault", "Arkana", 2022, 2800000, 1, "SUV", "1.3 Turbo", 26000, "Практичный городской кроссовер с экономичным двигателем."),
        new("Renault", "Megane", 2021, 2200000, 3, "Хэтчбек", "1.6", 34000, "Комфортный европейский хэтчбек для города."),
        new("Peugeot", "3008", 2022, 3100000, 1, "SUV", "1.6 Turbo", 21000, "Стильный французский кроссовер с современным интерьером."),
        new("Peugeot", "508", 2023, 3600000, 2, "Седан", "1.8 Turbo", 14000, "Элегантный бизнес-седан с мягкой подвеской."),
        new("Citroen", "C5 Aircross", 2021, 2700000, 1, "SUV", "1.6", 39000, "Комфортный кроссовер с фирменной мягкостью хода."),
        new("Citroen", "C4", 2022, 2300000, 3, "Хэтчбек", "1.2 Turbo", 28000, "Городской автомобиль с необычным дизайном."),
        new("Skoda", "Kodiaq", 2023, 3400000, 1, "SUV", "2.0 TSI", 17000, "Надежный семейный SUV с вместительным салоном."),
        new("Skoda", "Octavia", 2022, 2500000, 2, "Седан", "1.4 TSI", 26000, "Практичный седан с большим багажником."),
        new("SEAT", "Ateca", 2022, 2600000, 1, "SUV", "1.5 TSI", 24000, "Сбалансированный кроссовер для активной семьи."),
        new("SEAT", "Leon", 2023, 2400000, 3, "Хэтчбек", "1.5 TSI", 16000, "Динамичный хэтчбек с европейскими настройками шасси."),
        new("Mitsubishi", "Outlander", 2023, 3200000, 1, "SUV", "2.5", 15000, "Семиместный кроссовер для дальних поездок."),
        new("Mitsubishi", "Lancer", 2021, 2100000, 2, "Седан", "1.8", 43000, "Надежный седан с недорогим обслуживанием."),
        new("Suzuki", "Vitara", 2022, 2200000, 1, "SUV", "1.4 BoosterJet", 27000, "Компактный кроссовер для городской эксплуатации."),
        new("Suzuki", "Swift", 2023, 1800000, 3, "Хэтчбек", "1.2", 14000, "Маневренный и экономичный городской хэтчбек."),
        new("Jeep", "Grand Cherokee", 2022, 5800000, 1, "SUV", "3.6 V6", 22000, "Внедорожник с высоким уровнем проходимости и комфорта."),
        new("Jeep", "Wrangler", 2023, 6200000, 1, "SUV", "2.0 Turbo", 12000, "Легендарный рамный внедорожник для приключений."),
        new("Dodge", "Charger", 2021, 5400000, 2, "Седан", "5.7 V8", 33000, "Мощный американский седан с харизмой muscle-car."),
        new("Dodge", "Challenger", 2022, 5900000, 7, "Купе", "6.4 V8", 18000, "Культовое купе с атмосферным V8."),
        new("GMC", "Yukon", 2023, 7600000, 1, "SUV", "6.2 V8", 9000, "Большой премиальный SUV для большой семьи."),
        new("GMC", "Terrain", 2022, 3500000, 1, "SUV", "2.0 Turbo", 21000, "Комфортный среднеразмерный кроссовер."),
        new("Infiniti", "QX60", 2023, 6900000, 8, "SUV", "3.5 V6", 13000, "Премиальный семиместный SUV с богатой отделкой."),
        new("Infiniti", "Q50", 2022, 4700000, 8, "Седан", "3.0 Turbo", 23000, "Динамичный премиальный седан с задним приводом."),
        new("Acura", "MDX", 2023, 6400000, 8, "SUV", "3.5 V6", 14000, "Технологичный кроссовер премиум-класса."),
        new("Acura", "TLX", 2022, 4600000, 8, "Седан", "2.0 Turbo", 21000, "Спортивный характер и японская надежность."),
        new("Jaguar", "F-Pace", 2023, 7300000, 8, "SUV", "2.0 Turbo", 12000, "Британский премиум-кроссовер с драйверским характером."),
        new("Jaguar", "XE", 2021, 4300000, 8, "Седан", "2.0 Turbo", 29000, "Стильный седан с точной управляемостью."),
        new("Land Rover", "Range Rover Sport", 2023, 13200000, 8, "SUV", "3.0 Diesel", 9000, "Люксовый SUV с выдающейся проходимостью."),
        new("Land Rover", "Discovery", 2022, 9800000, 8, "SUV", "3.0 Diesel", 17000, "Универсальный премиальный внедорожник."),
        new("Mini", "Countryman", 2022, 3600000, 1, "SUV", "2.0", 19000, "Компактный премиальный кроссовер с ярким стилем."),
        new("Mini", "Cooper S", 2023, 3300000, 3, "Хэтчбек", "2.0 Turbo", 11000, "Эмоциональный хэтчбек для города и трассы."),
        new("Alfa Romeo", "Stelvio", 2022, 5200000, 8, "SUV", "2.0 Turbo", 20000, "Итальянский кроссовер с ярким дизайном."),
        new("Alfa Romeo", "Giulia", 2023, 5100000, 8, "Седан", "2.0 Turbo", 13000, "Спортивный седан с идеальным балансом шасси."),
        new("Maserati", "Levante", 2023, 12100000, 8, "SUV", "3.0 V6", 10000, "Люксовый SUV с фирменным итальянским характером."),
        new("Maserati", "Ghibli", 2022, 9800000, 8, "Седан", "3.0 V6", 18000, "Премиальный седан с мощным звучанием мотора."),
        new("Aston Martin", "DBX", 2023, 24800000, 8, "SUV", "4.0 V8", 6000, "Роскошный британский SUV с динамикой спорткара."),
        new("Aston Martin", "Vantage", 2022, 21400000, 7, "Купе", "4.0 V8", 7000, "Эффектное купе для ценителей скорости."),
        new("McLaren", "GT", 2022, 27600000, 7, "Купе", "4.0 V8", 5000, "Гран-туризмо с карбоновой архитектурой."),
        new("McLaren", "Artura", 2023, 29500000, 7, "Купе", "3.0 Hybrid", 3000, "Гибридный суперкар нового поколения."),
        new("Genesis", "GV80", 2023, 6700000, 8, "SUV", "3.5 Turbo", 9000, "Премиальный кроссовер с роскошным салоном."),
        new("Genesis", "G80", 2022, 5200000, 8, "Седан", "2.5 Turbo", 17000, "Представительский седан с высоким уровнем оснащения.")
    ];

    private sealed record CarSeedItem(
        string Brand,
        string Model,
        int Year,
        decimal Price,
        int CategoryId,
        string BodyType,
        string EngineType,
        int Mileage,
        string Description);
}
