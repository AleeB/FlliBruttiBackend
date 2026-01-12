using FlliBrutti.Backend.Application.Responses;
using FlliBrutti.Backend.Core.Enums;
using FlliBrutti.Backend.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlliBrutti.Backend.Application.Extensions
{
    public static class MappersExtension
    {
        public static FirmaResponseDTO ToResponseDTO(this Firma firma)
        {
            return new FirmaResponseDTO
            {
                Idfirma = firma.Idfirma,
                Entrata = firma.Entrata,
                Uscita = firma.Uscita,
                IdUser = firma.IdUser,
                UserEmail = firma.IdUserNavigation?.Email,
                UserName = firma.IdUserNavigation?.IdPersonNavigation?.Name,
                UserSurname = firma.IdUserNavigation?.IdPersonNavigation?.Surname
            };
        }

        public static PersonResponseDTO ToResponseDTO(this Person person)
        {
            return new PersonResponseDTO
            {
                IdPerson = person.IdPerson,
                Name = person.Name,
                Surname = person.Surname,
                DOB = person.DOB,
                HasUserAccount = person.User != null,
                HasNonAuthenticatedAccount = person.UserNotAuthenticated != null
            };
        }

        public static UserResponseDTO ToResponseDTO(this User user)
        {
            return new UserResponseDTO
            {
                IdPerson = user.IdPerson,
                Email = user.Email,
                Type = (EType)user.Type,
                Name = user.IdPersonNavigation?.Name,
                Surname = user.IdPersonNavigation?.Surname,
                DOB = user.IdPersonNavigation?.DOB,
                TotalFirmeCount = user.Firme?.Count ?? 0,
                TotalPreventiviCount = user.PreventiviNcc?.Count ?? 0
            };
        }

        public static UserNotAuthenticatedResponseDTO ToResponseDTO(this UserNotAuthenticated user)
        {
            return new UserNotAuthenticatedResponseDTO
            {
                IdPerson = user.IdPerson,
                Email = user.Email,
                Ip = user.Ip,
                Name = user.IdPersonNavigation?.Name,
                Surname = user.IdPersonNavigation?.Surname,
                DOB = user.IdPersonNavigation?.DOB,
                TotalPreventiviCount = user.PreventiviNcc?.Count ?? 0
            };
        }

        public static PreventivoNCCResponseDTO ToResponseDTO(this PreventivoNCC preventivo)
        {
            return new PreventivoNCCResponseDTO
            {
                IdPreventivo = preventivo.IdPreventivo,
                Description = preventivo.Description,
                Costo = preventivo.Costo,
                IsTodo = preventivo.IsTodo,
                Partenza = preventivo.Partenza,
                Arrivo = preventivo.Arrivo,
                IdUser = preventivo.IdUser,
                UserEmail = preventivo.IdUserNavigation?.Email,
                UserName = preventivo.IdUserNavigation?.IdPersonNavigation?.Name,
                UserSurname = preventivo.IdUserNavigation?.IdPersonNavigation?.Surname,
                IdUserNonAutenticato = preventivo.IdUserNonAutenticato,
                NonAuthUserEmail = preventivo.IdUserNonAutenticatoNavigation?.Email,
                NonAuthUserName = preventivo.IdUserNonAutenticatoNavigation?.IdPersonNavigation?.Name,
                NonAuthUserSurname = preventivo.IdUserNonAutenticatoNavigation?.IdPersonNavigation?.Surname,
                NonAuthUserIp = preventivo.IdUserNonAutenticatoNavigation?.Ip,
                Extra = preventivo.PreventivoExtra?.Select(e => e.ToResponseDTO()).ToList() ?? new List<PreventivoExtraResponseDTO>()
            };
        }

        public static PreventivoExtraResponseDTO ToResponseDTO(this PreventivoExtra extra)
        {
            return new PreventivoExtraResponseDTO
            {
                Id = extra.Id,
                Costo = extra.Costo,
                Description = extra.Description,
                IdPreventivo = extra.IdPreventivo
            };
        }
    }
}
