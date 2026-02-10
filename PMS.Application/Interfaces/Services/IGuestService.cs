using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Guests;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.Interfaces.Services
{
    public interface IGuestService
    {
		Task<ResponseObjectDto<GuestDto>> AddGuestAsync(CreateGuestDto dto);
		Task<IEnumerable<GuestDto>> GetAllGuestsAsync(string? search);

		Task<ResponseObjectDto<GuestDto>> UpdateGuestAsync(int id, UpdateGuestDto dto);
		Task<ResponseObjectDto<bool>> DeleteGuestAsync(int id);
		//Task<ResponseObjectDto<IEnumerable<GuestSearchDto>>> SearchGuestsAsync(string searchTerm);
	}
}
