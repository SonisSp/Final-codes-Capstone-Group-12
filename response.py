import time
import yt_dlp

def measure_response_time(url):
    start_time = time.perf_counter()
    try:
        yt_dlp.main(['--simulate', url])
    except SystemExit:
        # yt-dlp calls sys.exit(), so we catch it to continue
        pass
    end_time = time.perf_counter()
    total_time = end_time - start_time
    return total_time

if __name__ == "__main__":
    test_url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ"
    response_time = measure_response_time(test_url)
    print(f"\nResponse time: {response_time:.2f} seconds")
