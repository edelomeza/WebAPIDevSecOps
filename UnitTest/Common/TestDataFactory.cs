using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPIDevSecOps.Dto;
using WebAPIDevSecOps.Models;

namespace UnitTest.Common
{
    public static class TestDataFactory
    {
        public static CliCliente CreateCliente(string? nombre = null, string? correo = null, string? telefono = null)
        {
            return new CliCliente
            {
                strNombreCliente = nombre ?? $"cliente{Guid.NewGuid():N}"[..20],
                strDireccionCliente = "Dirección de prueba",
                strCorreoElectronico = correo ?? $"cliente{Guid.NewGuid():N}@test.com",
                strNumeroTelefono = telefono ?? "5512345678",
                RowVersion = new byte[] { 1, 0, 0, 0 },
            };
        }

        public static List<CliCliente> CreateClientes(int count)
        {
            return Enumerable.Range(1, count)
                .Select(i => new CliCliente
                {
                    strNombreCliente = $"cliente{i}",
                    strDireccionCliente = $"Dirección {i}",
                    strCorreoElectronico = $"cliente{i}@test.com",
                    strNumeroTelefono = $"55{i:D8}",
                    RowVersion = new byte[] { 1, 0, 0, 0 },
                })
                .ToList();
        }

        public static CliClienteCreateDto CreateClienteCreateDto(string? nombre = null, string? correo = null, string? telefono = null, string? direccion = null)
        {
            return new CliClienteCreateDto
            {
                strNombreCliente = nombre ?? $"testcliente{Guid.NewGuid():N}"[..20],
                strCorreoElectronico = correo ?? $"cli{Guid.NewGuid():N}@test.com",
                strNumeroTelefono = telefono ?? "5512345678",
                strDireccionCliente = direccion,
            };
        }

        public static CliClienteUpdateDto CreateClienteUpdateDto(string? nombre = null, string? correo = null, string? telefono = null, string? direccion = null, byte[]? rowVersion = null)
        {
            return new CliClienteUpdateDto
            {
                strNombreCliente = nombre ?? $"updatedcli{Guid.NewGuid():N}"[..20],
                strCorreoElectronico = correo ?? $"updatedcli{Guid.NewGuid():N}@test.com",
                strNumeroTelefono = telefono ?? "5598765432",
                strDireccionCliente = direccion,
                RowVersion = rowVersion ?? new byte[] { 1, 0, 0, 0 },
            };
        }

        public static CliClienteDeleteDto CreateClienteDeleteDto(byte[]? rowVersion = null)
        {
            return new CliClienteDeleteDto
            {
                RowVersion = rowVersion ?? new byte[] { 1, 0, 0, 0 },
            };
        }

        public static SegUsuario CreateUser(string password)
        {
            return new SegUsuario
            {
                strNombre = "admin",
                strPWD = BCrypt.Net.BCrypt.HashPassword(password),
                strCorreoElectronico = "admin@test.com",
                RowVersion = new byte[] { 1 }
            };
        }

        public static List<SegUsuario> CreateUsers(int count, string? passwordHash = null)
        {
            passwordHash ??= BCrypt.Net.BCrypt.HashPassword("password");
            return Enumerable.Range(1, count)
                .Select(i => new SegUsuario
                {
                    strNombre = $"user{i}",
                    strPWD = passwordHash,
                    strCorreoElectronico = $"user{i}@test.com",
                    RowVersion = new byte[] { 1, 0, 0, 0 }
                })
                .ToList();
        }

        public static UsuarioCreateDto CreateUsuarioCreateDto(string? nombre = null, string? password = null, string? correo = null)
        {
            return new UsuarioCreateDto
            {
                strNombre = nombre ?? $"testuser_{Guid.NewGuid():N}"[..20],
                strPWD = password ?? "TestPass123!",
                strCorreoElectronico = correo ?? $"user_{Guid.NewGuid():N}@test.com"
            };
        }

        public static UsuarioUpdateDto CreateUsuarioUpdateDto(string? nombre = null, string? password = null, string? correo = null, byte[]? rowVersion = null)
        {
            return new UsuarioUpdateDto
            {
                strNombre = nombre ?? $"updateduser_{Guid.NewGuid():N}"[..20],
                strPWD = password,
                strCorreoElectronico = correo ?? $"updated_{Guid.NewGuid():N}@test.com",
                RowVersion = rowVersion ?? new byte[] { 1, 0, 0, 0 }
            };
        }

        public static UsuarioDeleteDto CreateUsuarioDeleteDto(byte[]? rowVersion = null)
        {
            return new UsuarioDeleteDto
            {
                RowVersion = rowVersion ?? new byte[] { 1, 0, 0, 0 }
            };
        }

        public static ProProducto CreateProducto(string? nombre = null, int existencia = 10, decimal precio = 99.99m)
        {
            return new ProProducto
            {
                strNombreProducto = nombre ?? $"producto{Guid.NewGuid():N}"[..20],
                strURLImagen = null,
                strDescripcion = "Descripción de prueba",
                intNumeroExistencia = existencia,
                decPrecio = precio,
                RowVersion = new byte[] { 1, 0, 0, 0 },
            };
        }

        public static List<ProProducto> CreateProductos(int count)
        {
            return Enumerable.Range(1, count)
                .Select(i => new ProProducto
                {
                    strNombreProducto = $"producto{i}",
                    strURLImagen = null,
                    strDescripcion = $"Descripción {i}",
                    intNumeroExistencia = i * 10,
                    decPrecio = i * 9.99m,
                    RowVersion = new byte[] { 1, 0, 0, 0 },
                })
                .ToList();
        }

        public static ProductoCreateDto CreateProductoCreateDto(string? nombre = null, int existencia = 10, decimal precio = 99.99m)
        {
            return new ProductoCreateDto
            {
                strNombreProducto = nombre ?? $"testproducto{Guid.NewGuid():N}"[..20],
                intNumeroExistencia = existencia,
                decPrecio = precio,
            };
        }

        public static ProductoUpdateDto CreateProductoUpdateDto(int id, string? nombre = null, int existencia = 20, decimal precio = 199.99m, byte[]? rowVersion = null)
        {
            return new ProductoUpdateDto
            {
                id = id,
                strNombreProducto = nombre ?? $"updatedproducto{Guid.NewGuid():N}"[..20],
                intNumeroExistencia = existencia,
                decPrecio = precio,
                RowVersion = rowVersion ?? new byte[] { 1, 0, 0, 0 },
            };
        }

        public static ProductoDeleteDto CreateProductoDeleteDto(int id, byte[]? rowVersion = null)
        {
            return new ProductoDeleteDto
            {
                id = id,
                RowVersion = rowVersion ?? new byte[] { 1, 0, 0, 0 },
            };
        }
    }
}
