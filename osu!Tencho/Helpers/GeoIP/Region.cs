using System;
using System.IO;
internal class Region{
  internal String countryCode;
  internal String countryName;
  internal String region;
  internal Region(){
  }
  internal Region(String countryCode,String countryName,String region){
      this.countryCode = countryCode;
      this.countryName = countryName;
      this.region = region;
  }
  internal String getcountryCode() {
      return countryCode;
  }
  internal String getcountryName() {
      return countryName;
  }
  internal String getregion() {
      return region;
  }
}

