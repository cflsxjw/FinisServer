using System.ComponentModel.DataAnnotations;

namespace FinisServer.Models.Dtos;

public record TextEmbeddingPostDto(
    [Required] string Model,
    [Required] TextEmbeddingInput Input,
    [Required] string OutputType);
public record TextEmbeddingInput(IList<string> Texts);

