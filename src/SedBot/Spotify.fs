module SedBot.Spotify

open System
open System.Net.Http
open SedBot.Common
open SpotifyAPI.Web

let getTracks () =
    let spotify = SpotifyClient("YourAccessToken")
    ()
