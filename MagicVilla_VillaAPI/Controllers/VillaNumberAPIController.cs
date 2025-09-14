using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaNumberAPI")]
    [ApiController]
    public class VillaNumberAPIController : ControllerBase
    {
        protected APIResponse _response;

        private readonly IVillaNumberRepository _db;
        private readonly IMapper _mapper;
        public VillaNumberAPIController(IVillaNumberRepository db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            this._response = new();
        }
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetVillasNumber()
        {
            try
            {

                IEnumerable<VillaNumber> VillaNumberList = await _db.GetAll();
                _response.Result = _mapper.Map<List<VillaNumberDTO>>(VillaNumberList);
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
        [HttpGet("{id:int}", Name = "GetVillaNumber")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> GetVillaNumber(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;

                    return BadRequest(_response);
                }
                var villa = await _db.Get(u => u.VillaNo == id);
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
        public async  Task<ActionResult<APIResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDTO createDTO)
        {
            try
            {
               
                if (_db.Get(u => u.VillaNo == createDTO.VillaNo) != null)
                {
                    ModelState.AddModelError("Custom Error", "vills Number already Exists");
                    return BadRequest(ModelState);
                }
                if (createDTO == null)
                {
                    return BadRequest();
                }
               
                VillaNumber model = _mapper.Map<VillaNumber>(createDTO);
                
                await _db.Create(model);
                _response.Result = _mapper.Map<VillaNumberDTO>(model);
                _response.StatusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetVillaNumber", new { id = model.VillaNo }, _response);
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
        [HttpDelete("{id}", Name = "DeleteVillaNumber")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public  async Task <ActionResult<APIResponse>> DeleteNumber(int id)
        {
            try { 
             
            var villa = await _db.Get(u => u.VillaNo == id);
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
        [HttpPut("{id}", Name = "UpdateVillaNumber")]
        public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int id, [FromBody] VillNumberUpdateDTO updateDTO)
        {
            try { 
            if(updateDTO == null || id == updateDTO.VillaNo)
            {
                return BadRequest();
            }
            
            VillaNumber model = _mapper.Map<VillaNumber>(updateDTO);

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
        [HttpPatch("{id}", Name = "UpdatePartialVillaNumber")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePartialVillaNumber(int id , JsonPatchDocument<VillNumberUpdateDTO> patchDto)
        {
            if(patchDto == null || id == 0)
            {
                return BadRequest();
            }
            var villa =await _db.Get(u => u.VillaNo == id,tracked:false);

            VillNumberUpdateDTO villaDTO =_mapper.Map<VillNumberUpdateDTO>(villa);
           
            if(villa == null)
            {
                return BadRequest();
            }
            patchDto.ApplyTo(villaDTO, ModelState);
            VillaNumber model = _mapper.Map<VillaNumber>(villaDTO);

            await _db.Update(model);
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent() ;
        }
    }
}
