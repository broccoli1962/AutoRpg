# Local LLM Models

Place GGUF weights here for Phase 3+ narration PoC.

## Required file (default)

`Qwen2.5-1.5B-Instruct-Q4_K_M.gguf`

Expected by `LlmNarrationManager` / `LlmSmokeTest`.

Example (Hugging Face):

```text
https://huggingface.co/Qwen/Qwen2.5-1.5B-Instruct-GGUF
```

`.gguf` files are gitignored. Without a model, the game uses template narration fallback and stage play continues.
