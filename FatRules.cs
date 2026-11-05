namespace BurebistaCandleRecipes;
internal static class FatRules {
 internal static bool IsAnimalFat(string key){
  if(string.IsNullOrWhiteSpace(key)||!key.StartsWith("GEAR_",StringComparison.Ordinal))return false;string s=key[5..].ToLowerInvariant();
  if(s is "fat" or "fatraw" or "fatrendered" or "renderedfat" or "animalfat" or "lard" or "tallow" or "animalfatraw" or "animalfatrendered" or "renderedanimalfat")return true;
  bool fat=s.EndsWith("fat")||s.EndsWith("tallow")||s.EndsWith("lard");return fat&&new[]{"beaver","bear","moose","deer","wolf","rabbit","cougar","ptarmigan","boar","goose","animal"}.Any(s.Contains);
 }
}
