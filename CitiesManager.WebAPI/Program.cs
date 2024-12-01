using CitiesManager.Core.Domain.Identity;
using CitiesManager.Core.Service;
using CitiesManager.Core.ServiceContracts;
using CitiesManager.Infrastructure.DataBaseContext;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(
    opt=>
    {
        opt.Filters.Add(new ProducesAttribute("application/json"));
        // this global filter will make sure that all action method will send 
        // response to client as application/json.
        // ProducesAttribute represents response body type.
        opt.Filters.Add(new ConsumesAttribute("application/json"));
        // this global filter will make sure that all action method will accept 
        // request from client as application/json only.
        // ConsumesAttribute represents request body type.

        // Authrization Policy
        var policy= new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        opt.Filters.Add(new AuthorizeFilter(policy));
        // this code make AuthorizeFilter attribute globally
        // so we dont have to add it explicitly for each controllers or actions
        // it will also affect Account Controller so dont forget to add [AllowAnonymous]
        // attribute on Account Controller.

    }
    ).AddXmlSerializerFormatters();// by default xml Serialization wont supp in asp.net core
                                   // if we need we have to enable it likw this.


builder.Services.AddApiVersioning(config=>
{
    config.ApiVersionReader = new UrlSegmentApiVersionReader();
    // above line if for reading version number from route
    // also dont mention to mention this in controller [Route("api/v{version:apiVersion}/[controller]")]
    // inorder to fetch version number.
});

builder.Services.AddDbContext<ApplicationDbContext>(opt=>opt.UseSqlServer(builder.Configuration.GetConnectionString("CitiesConnectionString")));

// enable Swagger
builder.Services.AddEndpointsApiExplorer(); // enable swagget to access endpoints.
builder.Services.AddSwaggerGen(opt=>
{
    opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api.xml"));
    // generate openapi Specification (documentation).
    // opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,"api.xml") this include api.xml in documentation.
    // api.xml we can use to generate commented documentation details in action methods.


    //In order to versoning work on swagger, we have to create swagger docs for each version.
    opt.SwaggerDoc("v1", new OpenApiInfo() { Title = "Cities Web API", Version = "1.0" });
    opt.SwaggerDoc("v2", new OpenApiInfo() { Title = "Cities Web API", Version = "2.0" });

});
builder.Services.AddVersionedApiExplorer(opt=>
{
    opt.GroupNameFormat = "'v'VVV";
    // here small v is v and V is versionnumber, each V is for each digit.
    // here up to 3 digit will supp
    // AddVersionedApiExplorer we have to configure this else version number wont be identified.
});

// adding cors
builder.Services.AddCors(opt => {
    opt.AddDefaultPolicy(builder => {
        builder.WithOrigins("https://localhost:4200")
        .WithHeaders("Authorization", "origin", "accept", "content-type");
        // when we pass Authorization token in header from client(from angular means from diff server (cross server))
        // wep api by default wont allow it so we have to enable it in Cors
        // not only Authorization other headers as well like this.
    });
    // here we set header Access-Controll-Allow-Origin as  "https://localhost:4200"
    // which is server of out Angular application.

});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
    options.Password.RequiredLength = 5;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;

}).AddEntityFrameworkStores<ApplicationDbContext>()
  .AddDefaultTokenProviders()
  .AddUserStore<UserStore<ApplicationUser,ApplicationRole,ApplicationDbContext,Guid>>()
  .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext,Guid>>();

builder.Services.AddTransient<IJwtService, JwtService>();

builder.Services.AddAuthentication(opt => { 
    opt.DefaultAuthenticateScheme=JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; 
    // or we can give cookie auth here.
    // above two lines means if user is not authenticated with jwt it will redirect to
    // cookie AuthenticationScheme eg login page,login page dont need jwt Authentication.
}).AddJwtBearer(opt => {
    opt.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        // Above code says we have to validate Audience and Issuer with above values.
        ValidateLifetime=true,  
        ValidateIssuerSigningKey = true,//its the signature part in token
        IssuerSigningKey= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))

    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHsts();
app.UseHttpsRedirection();

app.UseSwagger();// create endpoints for swagger.json
app.UseSwaggerUI(opt =>
    {   // create UI as per the documentation for Swagger for each versions.
        opt.SwaggerEndpoint("/swagger/v1/swagger.json", "1.0");
        opt.SwaggerEndpoint("/swagger/v2/swagger.json", "2.0");
    }); // create UI as per the documentation for Swagger.
app.UseRouting();

app.UseCors();// add this  for adding header

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
