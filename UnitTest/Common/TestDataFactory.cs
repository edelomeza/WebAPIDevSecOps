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
    }
}
