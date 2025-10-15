import { API_BASE_URL } from "./config";

export interface GridResponse {
    boardId: string;
    grid: number[][];
    generation: number;
}

interface CreateBoardRequest {
  grid: number[][];
}

export async function getNextBoard(boardId: string): Promise<GridResponse> {
    const response = await fetch(`${API_BASE_URL}/board/${boardId}/next`);
    if (!response.ok) {
        throw new Error("Failed to fetch next generation");
    }
    const data = await response.json();

    return data.data;
}

export async function advanceNSteps(boardId: string, steps: number) {
    const response = await fetch(`${API_BASE_URL}/board/${boardId}/advance/${String(steps)}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    }
  });
    if (!response.ok) {
        throw new Error("Failed to advance generations.");
    }
}

export async function start(boardId: string) {
    const response = await fetch(`${API_BASE_URL}/board/${boardId}/start`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    }
  });
    if (!response.ok) {
        throw new Error("Failed to start the game.");
    }
}

export async function stop(boardId: string) {
    const response = await fetch(`${API_BASE_URL}/board/${boardId}/stop`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    }
  });
    if (!response.ok) {
        throw new Error("Failed to stop the game.");
    }
}

export async function createBoard(request: CreateBoardRequest): Promise<string> {
  const response = await fetch(`${API_BASE_URL}/board`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    throw new Error("Failed to create board");
  }

  const result = await response.json();

  if (result.data) {
    const { boardId } = result.data;

    return String(boardId);

  } else if (result.fail.errors) {
    throw new Error(result.fail.errors.join(", "));
  } else {
    throw new Error("Unexpected API response format");
  }
}

