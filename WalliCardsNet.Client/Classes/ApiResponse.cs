﻿namespace WalliCardsNet.Client.Classes
{
    public class ApiResponse<T>
    {
        public T Data { get; set; }
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
    }
}