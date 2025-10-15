import * as signalR from "@microsoft/signalr";
import { HUB_URL } from "../api/config";

export function createSignalRConnection() {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL)
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

  return connection;
}
