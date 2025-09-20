using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController :ControllerBase
    {
        protected APIResponse _response;

        private readonly IVillaRepository _db;
        private readonly IMapper _mapper;
        public VillaAPIController(IVillaRepository db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            this._response = new();
        }
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<APIResponse>> GetVillas()
        {
            try
            {

                IEnumerable<Villa> VillaList = await _db.GetAll();
                _response.Result = _mapper.Map<List<VillaDTO>>(VillaList);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex) {
                _response.IsSuccess = false;
                _response.ErrorMassage = new List<string>()
                {
                    ex.Message
                };
                return _response;
            }
        }
        [HttpGet("{id:int}", Name = "GetVilla")]
        [Authorize (Roles ="Admin")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;

                    return BadRequest(_response);
                }
                var villa = await _db.Get(u => u.Id == id);
                if (villa == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _response.Result = _mapper.Map<VillaDTO>(villa);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMassage = new List<string>()
                {
                    ex.Message
                };
                return _response;
            }
            }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async  Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDTO createDTO)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    return BadRequest();
                //}
                if (_db.Get(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
                {
                    ModelState.AddModelError("", "vills already Exists");
                    return BadRequest(ModelState);
                }
                if (createDTO == null)
                {
                    return BadRequest();
                }
                //if (villaDTO.Id > 0)
                //{
                //    return StatusCode(StatusCodes.Status500InternalServerError);
                //}
                Villa model = _mapper.Map<Villa>(createDTO);
                //Villa model = new()
                //{
                //    Name = createDTO.Name,

                //    Occupancy = createDTO.Occupancy,
                //    Amenity = createDTO.Amenity,
                //    ImageUrl = createDTO.ImageUrl,
                //    Details = createDTO.Details,
                //    Rate = createDTO.Rate,
                //    Sqft = createDTO.Sqft
                //};
                await _db.Create(model);
                _response.Result = _mapper.Map<VillaDTO>(model);
                _response.StatusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetVilla", new { id = model.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMassage = new List<string>()
                {
                    ex.Message
                };
                return _response;
            }
            }
        [HttpDelete("{id}", Name = "DeleteVilla")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles ="Custom")]
        public  async Task <ActionResult<APIResponse>> Delete(int id)
        {
            try { 
             
            var villa = await _db.Get(u => u.Id == id);
            if (villa != null)
            {
                await _db.Remove(villa);
                
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
                
            }
            else
            {
                return NotFound();
            }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMassage = new List<string>()
                {
                    ex.Message
                };
                return _response;
            }
        }
        [HttpPut("{id}", Name = "UpdateVilla")]
        [Authorize]
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody]VillaUpdateDTO updateDTO)
        {
            try { 
            if(updateDTO == null || id == updateDTO.Id)
            {
                return BadRequest();
            }
            //var villa = _db.Villas.FirstOrDefault(u=>u.Id == id);
            //villa.Name=villaDTO.Name;
            //villa.Sqft = villaDTO.Sqft;
            //villa.Occupancy = villaDTO.Occupancy;
            Villa model = _mapper.Map<Villa>(updateDTO);

            await _db.Update(model);


            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMassage = new List<string>()
                {
                    ex.Message
                };
                return _response;
            }
        }
        [HttpPatch("{id}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePartialVilla(int id , JsonPatchDocument<VillaUpdateDTO> patchDto)
        {
            if(patchDto == null || id == 0)
            {
                return BadRequest();
            }
            var villa =await _db.Get(u => u.Id == id,tracked:false);

            VillaUpdateDTO villaDTO =_mapper.Map<VillaUpdateDTO>(villa);
           
            if(villa == null)
            {
                return BadRequest();
            }
            patchDto.ApplyTo(villaDTO, ModelState);
            Villa model = _mapper.Map<Villa>(villaDTO);

            await _db.Update(model);
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent() ;
        }
    }
}
