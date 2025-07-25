using System;
using System.ComponentModel.DataAnnotations;

public class Livestream
{
  [Key]
  public Guid Id { get; set; } = Guid.NewGuid();
}
