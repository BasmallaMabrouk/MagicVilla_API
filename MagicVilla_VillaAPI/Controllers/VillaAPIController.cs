using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController :ControllerBase
    {

        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public VillaAPIController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas()
        {

            IEnumerable<Villa> VillaList = await _db.Villas.ToListAsync();

            return Ok(_mapper.Map<List<VillaDTO>>(VillaList));
        }
        [HttpGet("{id:int}", Name = "GetVilla")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VillaDTO>> GetVilla(int id)
        {
            if (id == 0)
            {
                
                return BadRequest();
            }
            var villa = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
            if (villa == null) { 
                return NotFound();
            }
            return Ok(_mapper.Map<VillaDTO>(villa));
        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async  Task<ActionResult<VillaDTO>> CreateVilla([FromBody] VillaCreateDTO createDTO)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest();
            //}
            if (_db.Villas.FirstOrDefault(u => u.Name.ToLower() == createDTO.Name.ToLower())!=null) {
                ModelState.AddModelError("", "vills already Exists");
                return BadRequest(ModelState);
            }
            if (createDTO == null) {
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
            _db.Villas.Add(model);
            await _db.SaveChangesAsync();
            return CreatedAtRoute("GetVilla",new {id =model.Id }, createDTO);
        }
        [HttpDelete("{id}", Name = "DeleteVilla")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public  async Task <IActionResult> Delete(int id)
        {
            var villa = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
            if (villa != null)
            {
                _db.Villas.Remove(villa);
               await _db.SaveChangesAsync();
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }
        [HttpPut("{id}", Name = "UpdateVilla")]
        public async Task<IActionResult> UpdateVilla(int id, [FromBody]VillaUpdateDTO updateDTO)
        {
            if(updateDTO == null || id == updateDTO.Id)
            {
                return BadRequest();
            }
            //var villa = _db.Villas.FirstOrDefault(u=>u.Id == id);
            //villa.Name=villaDTO.Name;
            //villa.Sqft = villaDTO.Sqft;
            //villa.Occupancy = villaDTO.Occupancy;
            Villa model = _mapper.Map<Villa>(updateDTO);
           
            _db.Villas.Update(model);

            await _db.SaveChangesAsync();
            return NoContent();
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
            var villa =await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);

            VillaUpdateDTO villaDTO =_mapper.Map<VillaUpdateDTO>(villa);
           
            if(villa == null)
            {
                return BadRequest();
            }
            patchDto.ApplyTo(villaDTO, ModelState);
            Villa model = _mapper.Map<Villa>(villaDTO);
            
            _db.Villas.Update(model);
            await _db.SaveChangesAsync();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent() ;
        }
    }
}
